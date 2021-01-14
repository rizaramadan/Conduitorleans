using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conduit.Features.Articles.Inputs;
using Conduit.Features.Articles.Outputs;
using Microsoft.AspNetCore.Authorization;
using Orleans;
using Contracts.Articles;
using Contracts.Users;
using Contracts;
using MediatR;

namespace Conduit.Features.Articles
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ArticlesController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IMediator _mediator;

        public ArticlesController(IMediator m, IClusterClient c)
        {
            _mediator = m;
            _client   = c;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string tag,
            [FromQuery] int? limit, 
            [FromQuery] int? offset
        )
        {
            (GetArticlesOutput Output, Error Error) = await _mediator.Send(new GetArticlesInput
            {
                Tag = tag,
                Limit = limit,
                Offset = offset
            });
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }
            return Ok(Output);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateArticleInput input) 
        {
            (CreateArticleOutput Output, Error Error) = await _mediator.Send(input);
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }
            return Ok(Output);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Get(string slug)
        {
            (Article Article, Error Error) = await _client.GetGrain<ISlugGrain>(slug).GetArticle();
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }
            return Ok(new { Article });
        }
    }
}
