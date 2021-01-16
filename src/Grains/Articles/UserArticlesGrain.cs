namespace Grains.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Articles;
    using Orleans;
    using Orleans.Runtime;

    using PersistenceState = Orleans.Runtime.IPersistentState<System.Collections.Generic.HashSet<long>>;

    public class UserArticlesGrain : BaseArticleGrain, IUserArticlesGrain
    {
        private readonly PersistenceState _articles;

        public UserArticlesGrain(
            [PersistentState(nameof(UserArticlesGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        ) : base(f)
        {
            _articles = s;
        }

        public async Task<Error> AddArticle(long articleId)
        {
            _articles.State.Add(articleId);
            await _articles.WriteStateAsync();
            return Error.None;
        }

        public async Task<Error> RemoveArticle(long articleId)
        {
            if (_articles.State.Contains(articleId))
            {
                _articles.State.Remove(articleId);
                await _articles.WriteStateAsync();
            }
            return Error.None;
        }

        public async Task<(List<long> ArticleId, string Auhtor, Error Error)> GetLatestArticle(int limit)
        {
            var result = _articles.State.ToList();
            return await Task.FromResult(
                (result.OrderByDescending(x => x).Take(limit).ToList()
                 , this.GetPrimaryKeyString()
                 , Error.None));
        }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetLatestArticlePair(string currentUser, int limit, int offset)
        {
            if (_articles.State == null)
            {
                return (null, 0, Error.None);
            }

            var list = _articles.State
                .Select(x => (x, this.GetPrimaryKeyString()))
                .ToList();
            var filtered = list.OrderByDescending(x => x)
                .Take(limit)
                .Skip(offset)
                .ToList();
            var filteredResult = await GetArticlesData(currentUser, list);
            return (filteredResult, Convert.ToUInt64(list.Count), Error.None);
        }
    }
}
