namespace Contracts.Favorites
{
    using Contracts.Articles;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IFavoritGrain : IGrainWithStringKey
    {
        Task<(long ArticleId, string Author, Error Error)> Favorite(string slug);
        Task<(long ArticleId, string Author, Error Error)> Unfavorite(string slug);

        Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticles(string currentUser, int limit, int offset);
    }
}
