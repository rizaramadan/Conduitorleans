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
        private readonly ILogger<UsersController> _logger;
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IClusterClient c, IJwtTokenGenerator g, IUserService s)
        {
            _logger = logger;
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterWrapper wrapper)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new Error("489523e8-f33d-478f-a6f8-b54d9fe7fae3", "invalid request"));
            var register = wrapper.User;
            var user = _client.GetGrain<IUserGrain>(register.Username);
            var error = await user.Register(register.Email, register.Password);
            if (error.Exist())
                return new JsonResult(error);
            return new JsonResult(new RegisterUserOutput(
                user.GetPrimaryKeyString(),
                register.Email,
                //TODO: update bio feature
                "some bio",
                //TODO: update image feature
                "some image",
                await _tokenGenerator.CreateToken(user.GetPrimaryKeyString())
            ));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginWrapper wrapper)
        {
            var login = wrapper.User;
            var (userId, error) = await _userService.GetUsernameByEmail(login.Email);
            if (error.Exist())
                return new JsonResult(error);
            var user = _client.GetGrain<IUserGrain>(userId);
            var errorLogin = await user.Login(login.Email, login.Password);
            if (errorLogin.Exist())
                return new JsonResult(errorLogin);
            return new JsonResult(new LoginUserOutput(
                user.GetPrimaryKeyString(),
                login.Email,
                //TODO: update bio feature
                "some bio",
                //TODO: update image feature
                "some image",
                await _tokenGenerator.CreateToken(user.GetPrimaryKeyString())
            ));
        }
    }
}
