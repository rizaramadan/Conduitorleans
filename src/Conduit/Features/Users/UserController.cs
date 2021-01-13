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
    using MediatR;

    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator m) => _mediator = m;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            (GetCurrentUserOutput Output, Error Error) = await _mediator.Send(new GetUser());
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Output);
        }

        [HttpPut]
        public async Task<IActionResult> Edit(UpdateUserWrapper u) 
        {
            (GetCurrentUserOutput Output, Error Error) = await _mediator.Send(u);
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Output);
        }
    }
}
