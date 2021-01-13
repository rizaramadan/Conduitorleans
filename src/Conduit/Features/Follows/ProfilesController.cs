namespace Conduit.Features.Follows
{
    using Contracts;
    using Contracts.Users;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Orleans;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ProfilesController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IMediator _mediator;

        public ProfilesController(IClusterClient c, IMediator m) 
        {
            _client = c;
            _mediator = m;
        }

        [AllowAnonymous]
        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            (User User, Error Error) = await _client.GetGrain<IUserGrain>(username).Get();
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new Profile 
            {
                Username  = username,
                Bio       = User.Bio,
                Image     = User.Image,
                Following = false
            });
        }

        [HttpPost("{username}/follow")]
        public async Task<IActionResult> Follow(string username)
        {
            (Profile Profile, Error Error) = await _mediator.Send(new Follow(username));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Profile);
        }

        [HttpDelete("{username}/follow")]
        public async Task<IActionResult> Unfollow(string username)
        {
            (Profile Profile, Error Error) = await _mediator.Send(new Unfollow(username));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(Profile);
        }
    }
}
