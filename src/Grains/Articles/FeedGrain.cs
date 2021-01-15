namespace Grains.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Articles;
    using Contracts.Follows;
    using Orleans;

    public class ArticleIdAuthor
    {
        public long ArticleId { get; }
        public string Author { get; }
        public ArticleIdAuthor(long i, string a)
        {
            ArticleId = i;
            Author = a;
        }
    }

    public class FeedGrain : BaseArticleGrain, IFeedGrain
    {
        public FeedGrain(IGrainFactory f) : base(f) { }
            
        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            Get(string currentUser, int limit, int offset)
        {
            var followingGrain = _factory.GetGrain<IUserFollowingGrain>(this.GetPrimaryKeyString());
            (HashSet<string> Following, Error Error) = await followingGrain.Get();
            if (Error.Exist())
            {
                return (null,0, Error);
            }
            var articlesTask = new List<Task<(List<long> ArticleId, string Author, Error Error)>>(Following.Count);
            foreach (var each in Following)
            {
                var userArticles = _factory.GetGrain<IUserArticlesGrain>(each);
                articlesTask.Add(userArticles.GetLatestArticle(limit));
            }
            var result = (await Task.WhenAll(articlesTask))
                .Where(x => !x.Error.Exist())
                .ToList();

            var articlePairs = new List<ArticleIdAuthor>();
            foreach (var eachList in result)
            {
                foreach (var each in eachList.ArticleId)
                {
                    articlePairs.Add(new ArticleIdAuthor(each,eachList.Author));
                }
            }
            var sortedArticlePairs = articlePairs
                .OrderByDescending(x => x.ArticleId)
                .Skip(offset)
                .Take(limit)
                .Select(x => (x.ArticleId, x.Author))
                .ToList();

            var output = await GetArticlesData(currentUser, sortedArticlePairs);
            var count = Convert.ToUInt64(articlePairs.Count);
            return (output, count, Error.None);
        }
    }
}
