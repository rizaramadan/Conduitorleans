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

    public class TagArticlesGrain : Grain, ITagArticlesGrain
    {
        private readonly IGrainFactory _factory;

        public TagArticlesGrain(IGrainFactory f) => _factory = f;


        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticlesByTag(string tag, int limit, int offset)
        {
            var tagGrain = _factory.GetGrain<ITagGrain>(tag);
            (List<(long, string)> ArticleIds, Error Error) tags =
                await tagGrain.GetArticles();
            var latest = tags.ArticleIds.OrderByDescending(x => x)
                .Skip(offset)
                .Take(limit)
                .ToList();

            var result = await ArticlesGrain.GetArticlesData(_factory, latest);
            var count = Convert.ToUInt64(tags.ArticleIds.Count);
            return (result, count, Error.None);
        }
    }
}
