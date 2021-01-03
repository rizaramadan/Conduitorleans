using Conduit.Infrastructure.Security;
using Conduit.Features.Users.Outputs;
using Contracts;
using Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System.Threading.Tasks;

namespace Conduit.Features.Users
{
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

            var user = _client.GetGrain<IUserGrain>(userId);

            return Ok(new GetCurrentUserOutput(
                user.GetPrimaryKeyString(),
                (await user.GetEmail()).Email,
                //TODO: update bio feature
                "some bio",
                //TODO: update image feature
                "some image",
                await _tokenGenerator.CreateToken(user.GetPrimaryKeyString())
            ));
        }
    }
}
