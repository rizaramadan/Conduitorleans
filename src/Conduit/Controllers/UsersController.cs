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

namespace Conduit.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IClusterClient _client;

        public UsersController(ILogger<UsersController> logger, IClusterClient c)
        {
            _logger = logger;
            _client = c;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterWrapper wrapper)
        {
            var register = wrapper.User;
            var user = _client.GetGrain<IUserGrain>(register.Username);
            var error = await user.Register(register.Email, register.Password);
            if (error.Exist())
                return new JsonResult(error);
            return new JsonResult(new RegisterUserOutput(
                user.GetPrimaryKeyString(),
                register.Email,
                "some bio",
                "some image",
                "some token"
            ));
        }
    }
}
