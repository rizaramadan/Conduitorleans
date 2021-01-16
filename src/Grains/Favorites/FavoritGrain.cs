namespace Grains.Favorites
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Favorites;
    using Grains.Articles;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using PersistenceState = Orleans.Runtime.IPersistentState<System.Collections.Generic.HashSet<ArticleIdentity>>;

    public class ArticleIdentity
    {
        public long Id { get; set; }
        public string Author { get; set; }
        public override bool Equals(object obj)
        {
            return obj is ArticleIdentity ai &&
                ai.Id == Id &&
                ai.Author.Equals(Author, StringComparison.Ordinal);
        }
        public override int GetHashCode()
        {
            return $"{Id}{Author}".GetHashCode();
        }
    }

    public class FavoritGrain : BaseArticleGrain, IFavoritGrain
    {
        private readonly PersistenceState _favoritState;

        public FavoritGrain(
            [PersistentState(nameof(FavoritGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        ) : base(f)
        {
            _favoritState = s;
        }
        public async Task<(long ArticleId, string Author, Error Error)> Favorite(string slug)
        {
            var slugGrain = _factory.GetGrain<ISlugGrain>(slug);
            (long ArticleId, string Author) = await slugGrain.GetArticleId();
            if (_favoritState.State == null)
            {
                _favoritState.State = new HashSet<ArticleIdentity>(1);
            }
            var articleGrain = _factory.GetGrain<IArticleGrain>(ArticleId, Author);
            await articleGrain.AddFavorited(Author);
            _favoritState.State.Add(new ArticleIdentity { Id = ArticleId, Author = Author });
            return (ArticleId, Author, Error.None);
        }

        public async Task<(long ArticleId, string Author, Error Error)> Unfavorite(string slug)
        {
            var slugGrain = _factory.GetGrain<ISlugGrain>(slug);
            (long ArticleId, string Author) = await slugGrain.GetArticleId();
            if (_favoritState.State != null)
            {
                _favoritState.State.Remove(new ArticleIdentity { Id = ArticleId, Author = Author });
            }
            var articleGrain = _factory.GetGrain<IArticleGrain>(ArticleId, Author);
            await articleGrain.RemoveFavorited(Author);
            return (ArticleId, Author, Error.None);
        }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticles(string currentUser, int limit, int offset)
        {
            if (_favoritState.State == null)
            {
                return (null, 0L, Error.None);
            }
            // List<(long ArticleId, string Author)>
            var all = _favoritState.State.Select(x => (x.Id, x.Author)).ToList();
            var filtered = all.OrderByDescending(x => x)
                .Take(limit)
                .Skip(offset)
                .ToList();
            var result = await GetArticlesData(currentUser, filtered);
            return (result, Convert.ToUInt64(all.Count), Error.None);
        }
    }
}
