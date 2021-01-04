namespace Conduit.Features.Articles.Inputs
{
    using Contracts.Articles;
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class CreateArticleInput : IArticle
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string[] TagList { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Author { get; set; }
        public List<IArticleFavorited> Favorited { get; set; }
        public int FavoritesCount { get; set; }
        List<string> IArticle.TagList { get; set; }
    }

    public class CreateArticleValidator : AbstractValidator<CreateArticleInput>
    {
        public CreateArticleValidator()
        {
            RuleFor(lw => lw.Title).NotEmpty();
            RuleFor(lw => lw.Description).NotEmpty();
            RuleFor(lw => lw.Body).NotEmpty();
        }
    }
}
