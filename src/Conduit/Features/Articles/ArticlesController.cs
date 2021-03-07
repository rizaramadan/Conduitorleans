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
    using Conduit.Features.Articles.Favorites;
    using System.Collections.Generic;
    using Contracts.Comments;
    using System;
    using Conduit.Features.Articles.Comments;

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

        [Authorize]
        [HttpPost("{slug}/favorite")]
        public async Task<IActionResult> Favorite([FromRoute]string slug)
        {
            (Article Article, Error Error) = await _mediator.Send(new Favorite(slug));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new { Article });
        }

        [Authorize]
        [HttpDelete("{slug}/favorite")]
        public async Task<IActionResult> Unfavorite([FromRoute] string slug)
        {
            (Article Article, Error Error) = await _mediator.Send(new Unfavorite(slug));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new { Article });
        }

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> Feed([FromQuery] int? limit, [FromQuery] int? offset) 
        {
            (GetArticlesOutput Output, Error Error) = await _mediator.Send(new GetArticlesInput
            {
                Feed = true,
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
        [HttpPost("{slug}/comments")]
        public async Task<IActionResult> Comments([FromRoute] string slug, [FromBody] CommentWrapper c)
        {
            (Comment Comment, Error Error) = await _mediator.Send(new CreateComment(slug, c.Comment));
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new { Comment = Comment });
        }

        [HttpGet("{slug}/comments")]
        public async Task<IActionResult> Comments([FromRoute] string slug)
        {
            (List<Comment> Comments, Error Error) = await (_client.GetGrain<ICommentsGrain>(slug)).Get(_userService.GetCurrentUsername(), slug);
            if (Error.Exist())
            {
                return UnprocessableEntity(Error);
            }

            return Ok(new { Comments = Comments });
        }


        [Authorize]
        [HttpDelete("{slug}/comments/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] string slug, [FromRoute] long id)
        {
            var error = await (_client.GetGrain<ICommentsGrain>(slug)).RemoveComment(_userService.GetCurrentUsername(),id, slug);
            if (error.Exist())
            {
                return UnprocessableEntity(error);
            }

            return Ok();
        }
    }
}
