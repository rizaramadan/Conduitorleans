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
        private const string KeyFormat = "yyyyMMddHHmmss";

        private readonly IClusterClient _client;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        public ArticlesController(IClusterClient c, IUserService u, IMediator m)
        {
            _client = c;
            _userService = u;
            _mediator = m;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int? limit, 
            [FromQuery] int? offset
        )
        {
            (GetArticlesOutput Output, Error Error) = await _mediator.Send(new GetArticlesInput
            {
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
            var (username, error) = _userService.GetCurrentUsername();
            if (error.Exist()) 
            {
                return UnprocessableEntity(error);
            }

            /// The key of article is compound of long and string. 
            /// integer is filled with yyyyMMddHHmmss parse as long
            /// string is filled with creator's userId as string
            /// this means a user can only create one article per second
            /// its still a reasonable limitation
            var key = long.Parse(DateTime.Now.ToString(KeyFormat));
            var grain = _client.GetGrain<IArticleGrain>(key, username);
            error = await grain.CreateArticle(new Article 
            {
                Title = input.Article.Title,
                Body = input.Article.Body,
                Description = input.Article.Description,
                TagList = input.Article.TagList
            });
            if (error.Exist())
            {
                return UnprocessableEntity(error);
            }

            (Article Article, Error Error) savedArticle = await grain.GetArticle();
            if (savedArticle.Error.Exist()) 
            {
                return UnprocessableEntity(error);
            }
            return Ok(new CreateArticleOutput { Article = savedArticle.Article });
        }
    }
}
