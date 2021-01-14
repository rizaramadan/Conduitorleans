namespace Contracts.Articles
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ISlugGrain : IGrainWithStringKey
    {
        Task<(long ArticleId, string Author)> GetArticleId();
        Task<(Article Article, Error Error)> GetArticle();
        Task<Error> DeleteArticle(string username);
    }
}
