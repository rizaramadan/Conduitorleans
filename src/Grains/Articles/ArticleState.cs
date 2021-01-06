namespace Grains.Articles
{
    using Contracts.Articles;
    using Contracts.Users;
    using System;
    using System.Collections.Generic;
    using System.Text;


    [Serializable]
    public class ArticleState : IArticle
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Description { get; set; }
        public List<string> TagList { get; set; }
        public string Author { get; set; }
        public List<IUser> Favorited { get; set; }
        public int FavoritesCount { get; set; }
    }
}
