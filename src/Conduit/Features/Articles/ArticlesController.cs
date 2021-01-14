namespace Conduit.Features.Articles
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Conduit.Features.Articles.Inputs;
    using Conduit.Features.Articles.Outputs;
    using Microsoft.AspNetCore.Authorization;
    using Orleans;
    using Contracts.Articles;
    using Contracts.Users;
    using Contracts;
    using MediatR;

    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ArticlesController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public ArticlesController(IMediator m, IClusterClient c, IUserService u)
        {
            _mediator = m;
            _client   = c;
            _userService = u;
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

        [HttpDelete("{slug}")]
        public async Task<IActionResult> Delete(string slug)
        {
            Error Error =
                await _client.GetGrain<ISlugGrain>(slug).DeleteArticle(_userService.GetCurrentUsername());
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok();
        }

        [HttpPut("{slug}")]
        public async Task<IActionResult> Edit([FromRoute]string slug, [FromBody] UpdateArticleWrapper u)
        {
            (Article Article, Error Error) = await _mediator.Send(u.SetSlug(slug));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok( new { Article });
        }
    }
}
