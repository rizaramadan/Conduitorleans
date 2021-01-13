namespace Conduit.Features.Users
{
    using Contracts;
    using Contracts.Users;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProfilesController : ControllerBase
    {
        private readonly IClusterClient _client;

        public ProfilesController(IClusterClient c) => _client = c;

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
    }
}
