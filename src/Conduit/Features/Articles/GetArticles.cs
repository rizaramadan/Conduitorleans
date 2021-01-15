namespace Conduit.Features.Articles
{
    using Conduit.Features.Articles.Outputs;
    using Contracts;
    using Contracts.Articles;
    using Contracts.Users;
    using MediatR;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetArticlesInput : IRequest<(GetArticlesOutput Output, Error Error)>
    {
        public const int MaxLimit = 10;

        public string Tag { get; set; }
        public string Author { get; set; }
        public string Favorited { get; set; }
        public bool Feed { get; set; }
        private int? _limit;
        public int? Limit 
        {
            get => _limit.HasValue ? _limit : MaxLimit;
            set => _limit = value > MaxLimit ? MaxLimit : value;
        }
        private int? _offset;
        public int? Offset
        {
            get => _offset.HasValue ? _offset : 0;
            set => _offset = value;
        }

        public bool AllAtricles() => 
            string.IsNullOrWhiteSpace(Tag) 
            && string.IsNullOrWhiteSpace(Author)
            && string.IsNullOrWhiteSpace(Favorited)
            && !Feed;

        internal bool ExistTagOnly() =>
            !string.IsNullOrWhiteSpace(Tag)
            && string.IsNullOrWhiteSpace(Author)
            && string.IsNullOrWhiteSpace(Favorited)
            && !Feed;
    }

    public class GetArticlesHandler : IRequestHandler<GetArticlesInput, 
                                      (GetArticlesOutput Output, Error Error)>
    {
        
        private static readonly Func<ArticleUserPair, GetArticleOutput> _converter = x =>
        {
            var output = new GetArticleOutput
            {
                Title          = x.Article.Title,
                Slug           = x.Article.Slug,
                Body           = x.Article.Body,
                CreatedAt      = x.Article.CreatedAt,
                UpdatedAt      = x.Article.UpdatedAt,
                Description    = x.Article.Description,
                TagList        = x.Article.TagList,
                Favorited      = x.Article.Favorited,
                FavoritesCount = x.Article.FavoritesCount,
                Author         = x.User
            };
            return output;
        };

        private readonly IClusterClient _client;
        private readonly IUserService _userService;

        public GetArticlesHandler(IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        public async Task<(GetArticlesOutput Output, Error Error)> 
            Handle(GetArticlesInput req, CancellationToken ct)
        {
            try
            {
                (List<ArticleUserPair> Articles, ulong Count, Error Error) result =
                req.AllAtricles() ?
                    await GetAllArticles(req.Limit.Value, req.Offset.Value)
                    : req.ExistTagOnly() ?
                        await GetArticlesByTag(req)
                        : req.Feed ?
                            await GetFeed(req.Limit.Value, req.Offset.Value)
                            : (null, 0, Error.None);

                if (result.Error.Exist())
                {
                    return (null, result.Error);
                }
                var articlesOutput = result.Articles.Select(x => _converter(x)).ToList();
                return
                (
                    new GetArticlesOutput
                    {
                        Articles = articlesOutput,
                        ArticlesCount = result.Count
                    },
                    Error.None
                );
            }
            catch (Exception ex)
            {
                return (null, new Error("7f9cef3e-ec24-45d9-b59d-f188ed1c6e5b", ex.Message));
            }
        }

        

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> 
            GetAllArticles(int limit, int offset)
        {
            var grains = _client.GetGrain<IArticlesGrain>(0);
            var username = _userService.GetCurrentUsername();
            var result = await grains.GetHomeGuestArticles(username, limit, offset);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticlesByTag(GetArticlesInput req)
        {
            var grains = _client.GetGrain<ITagArticlesGrain>(req.Tag);
            var username = _userService.GetCurrentUsername();
            var result = await grains.GetArticlesByTag(username, req.Limit.Value, req.Offset.Value);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> 
            GetFeed(int limit, int offset)
        {
            var username = _userService.GetCurrentUsername();
            var grains = _client.GetGrain<IFeedGrain>(username);
            var result = await grains.Get(username, limit, offset);
            return result;
        }
    }
}
