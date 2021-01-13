 using Conduit.Features.Users.Inputs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Security;
using Contracts;
using Conduit.Features.Users.Outputs;
using Conduit.Infrastructure.Security;
using Contracts.Users;
using MediatR;

namespace Conduit.Features.Users
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;

        public UsersController(IClusterClient c, IJwtTokenGenerator g, IUserService s, IMediator m)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
            _mediator = m;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterWrapper r)
        {
            (RegisterUserOutput Output, Error Error) = await _mediator.Send(r);
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Output);
        } 

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginWrapper l)
        {
            (LoginUserOutput Output, Error Error) = await _mediator.Send(l);
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Output);
        }
    }
}
