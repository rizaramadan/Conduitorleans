namespace Conduit.Features.Articles.Inputs
{
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CreateArticle
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string[] TagList { get; set; }
    }

    public class CreateArticleValidator : AbstractValidator<CreateArticle>
    {
        public CreateArticleValidator()
        {
            RuleFor(lw => lw.Title).NotEmpty();
            RuleFor(lw => lw.Description).NotEmpty();
            RuleFor(lw => lw.Body).NotEmpty();
        }
    }
}
