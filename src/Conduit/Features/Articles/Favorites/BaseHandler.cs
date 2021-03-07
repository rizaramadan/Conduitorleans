using Contracts;
using Contracts.Articles;
using Contracts.Favorites;
using Contracts.Users;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Features.Articles.Favorites
{
    public abstract class BaseHandler 
    {
        protected readonly IGrainFactory _client;
        protected readonly IUserService _userService;
        public BaseHandler(IGrainFactory c, IUserService u) 
        {
            _client = c;
            _userService = u;
        }

        public async Task<(Article Article, Error Error)> HandleBase(string slug, CancellationToken ct)
        {
            var currentUser = _userService.GetCurrentUsername();
            var favoritGrain = _client.GetGrain<IFavoritGrain>(currentUser);
            (long ArticleId, string Author, Error Error) = await Process(favoritGrain, slug);
            if (Error.Exist()) 
            {
                return (null, Error);
            }

            var grain = _client.GetGrain<IArticleGrain>(ArticleId, Author);
            return await grain.Get();
        }

        protected abstract Task<(long ArticleId, string Author, Error Error)> Process(IFavoritGrain g, string s); 
    }
}