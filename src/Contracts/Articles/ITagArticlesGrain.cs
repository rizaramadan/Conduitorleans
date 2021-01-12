namespace Contracts.Articles
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Orleans;

    public interface ITagArticlesGrain : IGrainWithStringKey
    {
        public Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticlesByTag(int limit, int offset);
    }
}
