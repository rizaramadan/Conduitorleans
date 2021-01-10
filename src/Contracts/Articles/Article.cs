using Contracts.Users;
using System;
using System.Collections.Generic;

namespace Contracts.Articles
{
    [Serializable]
    public class Article
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Description { get; set; }
        public List<string> TagList { get; set; }
        public string Author { get; set; }
        public List<string> Favorited { get; set; }
        public int FavoritesCount { get; set; }
    }
}

