namespace Conduit.Features.Articles.Inputs
{
    using Conduit.Features.Articles.Outputs;
    using Contracts;
    using Contracts.Articles;
    using Contracts.Users;
    using FluentValidation;
    using MediatR;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PostArticle
    {
        public object Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public List<string> TagList { get; set; }
    }

    public class CreateArticleValidator : AbstractValidator<CreateArticleInput>
    {
        public CreateArticleValidator()
        {
            RuleFor(lw => lw.Article.Title).NotEmpty();
            RuleFor(lw => lw.Article.Description).NotEmpty();
            RuleFor(lw => lw.Article.Body).NotEmpty();
        }
    }
    public class CreateArticleInput : IRequest<(CreateArticleOutput Output, Error Error)>
    {
        public PostArticle Article { get; set; }
    }

    public class CreateArticlesHandler : IRequestHandler<CreateArticleInput,
                                      (CreateArticleOutput Output, Error Error)>
    {
        private const string KeyFormat = "yyyyMMddHHmmss";

        private readonly IClusterClient _client;
        private readonly IUserService _userService;
        public CreateArticlesHandler(IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        public async Task<(CreateArticleOutput Output, Error Error)> 
            Handle(CreateArticleInput req, CancellationToken ct)
        {
            var (username, error) = _userService.GetCurrentUsername();
            if (error.Exist())
            {
                return (null, error);
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
                Title       = req.Article.Title,
                Body        = req.Article.Body,
                Description = req.Article.Description,
                TagList     = req.Article.TagList
            });
            if (error.Exist())
            {
                return (null, error);
            }

            (Article Article, Error Error) savedArticle = await grain.GetArticle();
            if (savedArticle.Error.Exist())
            {
                return (null, error);
            }
            return (new CreateArticleOutput { Article = savedArticle.Article }, Error.None);
        }
    }
}
