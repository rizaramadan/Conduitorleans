namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Tags;
    using Contracts.Users;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class TagArticlesGrain : BaseArticleGrain, ITagArticlesGrain
    {
        public TagArticlesGrain(IGrainFactory f) : base(f) { }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticlesByTag(int limit, int offset)
        {
            var tagGrain = _factory.GetGrain<ITagGrain>(this.GetPrimaryKeyString());
            (List<(long ArticleId, string Author)> ArticleIds, Error Error) tags =
                await tagGrain.GetArticles();
            var latest = tags.ArticleIds.OrderByDescending(x => x)
                .Skip(offset)
                .Take(limit)
                .ToList();

            var result = await GetArticlesData(latest);
            var count = Convert.ToUInt64(tags.ArticleIds.Count);
            return (result, count, Error.None);
        }
    }
}
