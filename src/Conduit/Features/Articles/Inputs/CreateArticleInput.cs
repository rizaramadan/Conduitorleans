namespace Conduit.Features.Articles.Inputs
{
    using Contracts.Articles;
    using Contracts.Users;
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CreateArticleInput
    {
        public Article Article { get; set; }
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
}
