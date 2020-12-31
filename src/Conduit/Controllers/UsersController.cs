 using Conduit.Models.Inputs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrainInterfaces.Security;
using GrainInterfaces;
using Conduit.Models.Outputs;
using Conduit.Infrastructure.Security;
using GrainInterfaces.Services;

namespace Conduit.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;

        public UsersController(IClusterClient c, IJwtTokenGenerator g, IUserService s)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterWrapper r)
        {
            var user = _client.GetGrain<IUserGrain>(r.User.Username);
            var error = await user.Register(r.User.Email, r.User.Password);
            if (error.Exist())
            {
                return new JsonResult(error);
            }

            return new JsonResult(new RegisterUserOutput(
                user.GetPrimaryKeyString(),
                r.User.Email,
                //TODO: update bio feature
                "some bio",
                //TODO: update image feature
                "some image",
                await _tokenGenerator.CreateToken(user.GetPrimaryKeyString())
            ));
        } 

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginWrapper l)
        {
            var (userId, error) = await _userService.GetUsernameByEmail(l.User.Email);
            if (error.Exist())
            {
                return new JsonResult(error);
            }

            var user = _client.GetGrain<IUserGrain>(userId);
            var errorLogin = await user.Login(l.User.Email, l.User.Password);
            if (errorLogin.Exist())
            {
                return new JsonResult(errorLogin);
            }

            return new JsonResult(new LoginUserOutput(
                user.GetPrimaryKeyString(),
                l.User.Email,
                //TODO: update bio feature
                "some bio",
                //TODO: update image feature
                "some image",
                await _tokenGenerator.CreateToken(user.GetPrimaryKeyString())
            ));
        }
    }
}
