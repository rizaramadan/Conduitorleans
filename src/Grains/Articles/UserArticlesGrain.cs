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

    public class UserArticlesGrain : Grain, IUserArticlesGrain
    {
        private readonly PersistenceState _articles;
        private readonly IGrainFactory _factory;

        public UserArticlesGrain(
            [PersistentState(nameof(UserArticlesGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _articles = s;
            _factory = f;
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

        public async Task<(List<long> ArticleId, Error Error)> GetLatestArticle(int limit)
        {
            var result = _articles.State.ToList();
            return await Task.FromResult(
                (result.OrderByDescending(x => x).Take(limit).ToList(), Error.None));
        }
    }
}
