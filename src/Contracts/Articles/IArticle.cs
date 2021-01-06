using Contracts.Users;
using System;
using System.Collections.Generic;

namespace Contracts.Articles
{
    public interface IArticle
    {
        string Title { get; set; }
        string Slug { get; set; }
        string Body { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        string Description { get; set; }
        List<string> TagList { get; set; }
        string Author { get; set; }
        List<IUser> Favorited { get; set; }
        int FavoritesCount { get; set; }
    }
}

