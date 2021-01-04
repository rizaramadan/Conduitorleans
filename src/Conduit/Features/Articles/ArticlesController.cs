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

namespace Conduit.Features.Articles
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ArticlesController : ControllerBase
    {
        private static readonly Error UserIdError =
            new Error("214630de-9fb8-4539-bbf6-738166078742", "userid not valid");

        private readonly IClusterClient _client;
        private readonly IUserService _userService;
        public ArticlesController(IClusterClient c, IUserService u)
        {
            _client = c;
        }

        [HttpGet]
        public async Task<IActionResult> Get(GetArticlesInput input)
        {
            return await  Task.FromResult(Ok(new ArticlesOutput())) ;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(CreateArticle input) 
        {
            var (userId, error) = _userService.GetCurrentUsername();
            if (error.Exist()) 
            {
                return UnprocessableEntity(error);
            }
            if (long.TryParse(userId, out var userIdLong))
            {
                //WIP
                return Ok();
            }
            else 
            {
                return UnprocessableEntity(UserIdError);
            }
        }
    }
}
