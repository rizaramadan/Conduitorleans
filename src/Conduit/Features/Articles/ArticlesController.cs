﻿using Microsoft.AspNetCore.Http;
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

namespace Conduit.Features.Articles
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ArticlesController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly IUserService _userService;
        public ArticlesController(IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        //[HttpGet]
        //public async Task<IActionResult> Get(GetArticlesInput input)
        //{
        //    return await Task.FromResult(Ok(new ArticlesOutput())) ;
        //}

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
            var key = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            var article = _client.GetGrain<IArticleGrain>(key, username);
            error = await article.CreateArticle(input.Article);
            if (error.Exist())
            {
                return UnprocessableEntity(error);
            }
            else 
            {
                return Ok(new CreateArticleOutput { Article = input.Article });
            }

        }
    }
}
