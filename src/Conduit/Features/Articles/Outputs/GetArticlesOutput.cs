using Contracts.Articles;
using Contracts.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Features.Articles.Outputs
{
    public class GetArticleOutput
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Description { get; set; }
        public List<string> TagList { get; set; }
        public ArticleAuthor Author { get; set; }
        public List<string> Favorited { get; set; }
        public int FavoritesCount { get; set; }
    }

    public class ArticleAuthor 
    {
        public string Username { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
    }

    public class GetArticlesOutput
    {
        public List<GetArticleOutput> Articles { get; set; }
        public ulong ArticlesCount { get; set; }
    }
}
