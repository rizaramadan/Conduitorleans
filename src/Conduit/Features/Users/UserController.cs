namespace Conduit.Features.Users
{
    using Conduit.Infrastructure.Security;
    using Conduit.Features.Users.Outputs;
    using Contracts;
    using Contracts.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Orleans;
    using System.Threading.Tasks;
    using Conduit.Features.Users.Inputs;

    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;

        public UserController(IClusterClient c, IJwtTokenGenerator g, IUserService s)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var (userId, error) = _userService.GetCurrentUsername();
            if (error.Exist())
            {
                return UnprocessableEntity(error);
            }

            var userGrain = _client.GetGrain<IUserGrain>(userId);
            (Contracts.Users.User User, Error Error) = await userGrain.Get();
            if (Error.Exist()) 
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new GetCurrentUserOutput(
                userGrain.GetPrimaryKeyString(),
                User.Email,
                User.Bio,
                User.Image,
                await _tokenGenerator.CreateToken(userGrain.GetPrimaryKeyString())
            ));
        }

        [HttpPut]
        public async Task<IActionResult> Edit(UpdateUserWrapper u) 
        {
            var (userId, error) = _userService.GetCurrentUsername();
            if (error.Exist())
            {
                return UnprocessableEntity(error);
            }

            var userGrain = _client.GetGrain<IUserGrain>(userId);
            error = await userGrain.Update(u.User);
            if (error.Exist()) 
            {
                return UnprocessableEntity(error);
            }
            var result = await userGrain.Get();
            if (result.Error.Exist()) 
            {
                return UnprocessableEntity(error);
            }
            return Ok(new GetCurrentUserOutput(
                userGrain.GetPrimaryKeyString(),
                result.User.Email,
                result.User.Bio,
                result.User.Image,
                await _tokenGenerator.CreateToken(userGrain.GetPrimaryKeyString())
            ));
        }
    }
}
