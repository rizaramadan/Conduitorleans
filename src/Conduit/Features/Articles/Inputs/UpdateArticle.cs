namespace Conduit.Features.Articles.Inputs
{
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

    public class UpdateArticle
    {
        public string InputSlug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
    }
    public class UpdateArticleValidator : AbstractValidator<UpdateArticleWrapper>
    {
        public UpdateArticleValidator()
        {
            RuleFor(uw => uw.Article).NotNull();
            RuleFor(uw => uw.Article.Body).NotNull().When(x => x.Article.Description == null && x.Article.Title == null);
            RuleFor(uw => uw.Article.Title).NotNull().When(x => x.Article.Description == null && x.Article.Body == null);
            RuleFor(uw => uw.Article.Description).NotNull().When(x => x.Article.Title == null && x.Article.Body == null);

        }
    }

    public class UpdateArticleWrapper  : IRequest<(Article Article, Error Error)>
    {
        public UpdateArticle Article { get; set; }

        public UpdateArticleWrapper SetSlug(string slug) 
        {
            Article.InputSlug = slug;
            return this;
        }
    }

    public class UpdateArticleWrapperHandler : IRequestHandler<UpdateArticleWrapper, (Article Article, Error Error)>
    {
        private readonly IClusterClient _client;
        private readonly IUserService _userService;

        public UpdateArticleWrapperHandler(IMediator m, IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        public async Task<(Article Article, Error Error)> Handle(UpdateArticleWrapper req, CancellationToken ct)
        {
            var slugGrain = _client.GetGrain<ISlugGrain>(req.Article.InputSlug);
            (long ArticleId, string Author) = await slugGrain.GetArticleId();
            var username = _userService.GetCurrentUsername();
            if (!username.Equals(Author, StringComparison.OrdinalIgnoreCase)) 
            {
                return (null, new Error("0C02FD95-8C59-410D-B0D9-C15E1E5A85BD","user changing not the author"));
            }

            var articleGrain = _client.GetGrain<IArticleGrain>(ArticleId, Author);
            await articleGrain.UpdateArticle(req.Article.Title, req.Article.Body, req.Article.Description);
            return await articleGrain.Get();
        }
    }

}
