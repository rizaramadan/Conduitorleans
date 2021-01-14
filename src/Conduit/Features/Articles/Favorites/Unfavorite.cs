namespace Conduit.Features.Articles.Favorites
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Favorites;
    using Contracts.Users;
    using MediatR;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;

    public class Unfavorite : IRequest<(Article Article, Error Error)>
    {
        public string slug { get; set; }

        public Unfavorite(string slug)
        {
            this.slug = slug;
        }
    }

    public class UnfavoriteHandler : BaseHandler, IRequestHandler<Unfavorite, (Article Article, Error Error)>
    {
        public UnfavoriteHandler(IClusterClient c, IUserService u) : base(c, u) { }

        public async Task<(Article Article, Error Error)> Handle(Unfavorite req, CancellationToken ct)
        {
            return await HandleBase(req.slug, ct);
        }

        protected override Task<(long ArticleId, string Author, Error Error)> Process(IFavoritGrain g, string s)
        {
            return g.Favorite(s);
        }
    }
}