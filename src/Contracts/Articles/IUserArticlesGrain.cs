namespace Contracts.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Orleans;

    public interface IUserArticlesGrain : IGrainWithStringKey
    {
        Task<Error> AddArticle(long articleId);
        Task<Error> RemoveArticle(long articleId);
        Task<(List<long> ArticleId, string Auhtor, Error Error)> GetLatestArticle(int limit);
    }
}
