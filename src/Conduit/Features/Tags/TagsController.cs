namespace Conduit.Features.Tags
{
    using Contracts;
    using Contracts.Tags;
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
    public class TagsController : ControllerBase
    {
        private readonly IClusterClient _client;
        public TagsController(IClusterClient c) => _client = c;

        [HttpGet]
        public async Task<IActionResult> Get() 
        {
            (List<string> Tags, Error Error) = await _client.GetGrain<ITagsGrain>(0).GetTags();
            if (Error.Exist()) 
            {
                return UnprocessableEntity(Error);
            }

            return Ok( new { Tags } );
        }
    }
}
