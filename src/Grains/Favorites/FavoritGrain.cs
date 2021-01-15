namespace Grains.Favorites
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Favorites;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
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

    public class FavoritGrain : Grain, IFavoritGrain
    {
        private readonly PersistenceState _favoritState;
        private readonly IGrainFactory _factory;

        public FavoritGrain(
            [PersistentState(nameof(FavoritGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _favoritState = s;
            _factory = f;
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
    }
}
