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
        public PostArticle Article { get; set; }
    }
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
}
