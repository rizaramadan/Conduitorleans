namespace Conduit.Features.Articles
{
    using Conduit.Features.Articles.Outputs;
    using Contracts;
    using Contracts.Articles;
    using Contracts.Favorites;
    using Contracts.Users;
    using MediatR;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ArticleFunc = System.Func<GetArticlesInput, System.Threading.Tasks.Task<(System.Collections.Generic.List<Contracts.Articles.ArticleUserPair> Articles, ulong Count, Contracts.Error Error)>>;

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

        private static readonly int ArticleListTypeCount = Enum.GetNames(typeof(ArticlesListType)).Length;
        private readonly IGrainFactory _client;
        private readonly IUserService _userService;
        private readonly IArticleService _articleService;
        private readonly Dictionary<ArticlesListType, ArticleFunc> FuncMap;

        public GetArticlesHandler(IGrainFactory c, IUserService u, IArticleService a)
        {
            _client = c;
            _userService = u;
            _articleService = a;
            FuncMap = new Dictionary<ArticlesListType, ArticleFunc>(ArticleListTypeCount)
            {
                [ArticlesListType.All]       = GetAllArticles,
                [ArticlesListType.ByTag]     = GetArticlesByTag,
                [ArticlesListType.Authored]  = GetAuthored,
                [ArticlesListType.Favorited] = GetFavorited,
                [ArticlesListType.Feed]      = GetFeed
            };

        }

        public async Task<(GetArticlesOutput Output, Error Error)> 
            Handle(GetArticlesInput req, CancellationToken ct)
        {
            try
            {
                var type = _articleService.GetListType(req);
                (List<ArticleUserPair> Articles, ulong Count, Error Error) result = await FuncMap[type](req);

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
            GetAllArticles(GetArticlesInput i)
        {
            var grains = _client.GetGrain<IArticlesGrain>(0);
            var username = _userService.GetCurrentUsername();
            var result = await grains.GetHomeGuestArticles(username, i.Limit.Value, i.Offset.Value);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetArticlesByTag(GetArticlesInput req)
        {
            var username = _userService.GetCurrentUsername();
            var grains = _client.GetGrain<ITagArticlesGrain>(req.Tag);
            var result = await grains.GetArticlesByTag(username, req.Limit.Value, req.Offset.Value);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> 
            GetFeed(GetArticlesInput i)
        {
            var username = _userService.GetCurrentUsername();
            var grains = _client.GetGrain<IFeedGrain>(username);
            var result = await grains.Get(username, i.Limit.Value, i.Offset.Value);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetAuthored(GetArticlesInput i)
        {
            var grains = _client.GetGrain<IUserArticlesGrain>(i.Author);
            var currentUser = _userService.GetCurrentUsername();
            var result = await grains.GetLatestArticlePair(currentUser, i.Limit.Value, i.Offset.Value);
            return result;
        }

        private async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)>
            GetFavorited(GetArticlesInput i)
        {
            var grains = _client.GetGrain<IFavoritGrain>(i.Favorited);
            var currentUser = _userService.GetCurrentUsername();
            var result = await grains.GetArticles(currentUser, i.Limit.Value, i.Offset.Value);
            return result;
        }
    }
}
