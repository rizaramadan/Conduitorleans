﻿using Contracts;
using Contracts.Articles;
using Contracts.Favorites;
using Contracts.Users;
using MediatR;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Features.Articles.Favorites
{
    public class Favorite : IRequest<(Article Article, Error Error)>
    {
        public string Slug { get; set; }
        public Favorite(string slug) => Slug = slug;
    }

    public class FavoriteHandler : BaseHandler, IRequestHandler<Favorite, (Article Article, Error Error)>
    {
        public FavoriteHandler(IGrainFactory c, IUserService u): base(c, u) { }

        public async Task<(Article Article, Error Error)> Handle(Favorite req, CancellationToken ct)
        {
            (Article Article, Error Error) result = await HandleBase(req.Slug, ct);
            result.Article.Favorited = true;
            return result;
        }

        protected override Task<(long ArticleId, string Author, Error Error)> Process(IFavoritGrain g, string s)
        {
            return g.Favorite(s);
        }
    }
}