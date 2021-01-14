namespace Contracts.Articles
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ISlugGrain : IGrainWithStringKey
    {
        Task<(Article Article, Error Error)> GetArticle();
    }
}
