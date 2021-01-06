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

    public class Article : IArticle
    {
        public string Title  { get; set; }
        public string Slug  { get; set; }
        public string Body  { get; set; }
        public DateTime CreatedAt  { get; set; }
        public DateTime UpdatedAt  { get; set; }
        public string Description  { get; set; }
        public List<string> TagList  { get; set; }
        public string Author  { get; set; }
        public List<IUser> Favorited  { get; set; }
        public int FavoritesCount  { get; set; }
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
