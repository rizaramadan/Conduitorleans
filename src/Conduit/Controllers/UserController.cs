namespace Conduit.Controllers
{
    using Conduit.Infrastructure.Security;
    using Conduit.Models.Outputs;
    using GrainInterfaces;
    using GrainInterfaces.Security;
    using GrainInterfaces.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [ApiController]
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
                return new JsonResult(error);
            }

            var user = _client.GetGrain<IUserGrain>(userId);

            return new JsonResult(new GetCurrentUserOutput(
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
