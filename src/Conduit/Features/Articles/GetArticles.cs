namespace Conduit.Features.Articles.Inputs
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
        public const int MaxLimit = 20;

        public string Tag { get; set; }
        public string Autor { get; set; }
        public string Favorited { get; set; }

        private int? _limit;
        public int? Limit 
        {
            get => _limit.HasValue ? _limit : 0;
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
        private readonly IClusterClient _client;
        private static readonly Func<Article, User, GetArticleOutput> _converter = (x,y) =>
        {
            var output = new GetArticleOutput
            {
                Title          = x.Title,
                Slug           = x.Slug,
                Body           = x.Body,
                CreatedAt      = x.CreatedAt,
                UpdatedAt      = x.UpdatedAt,
                Description    = x.Description,
                TagList        = x.TagList,
                Favorited      = x.Favorited,
                FavoritesCount = x.FavoritesCount,
                Author         = new ArticleAuthor 
                { 
                    Username = y.Username, 
                    Bio      = y.Bio, 
                    Image    = y.Image 
                }
            };
            return output;
        };

        public GetArticlesHandler(IClusterClient c) => _client = c;

        public async Task<(GetArticlesOutput Output, Error Error)> 
            Handle(GetArticlesInput req, CancellationToken ct)
        {
            try
            {
                var articlesGrain = _client.GetGrain<IArticlesGrain>(0);
                var articles = await articlesGrain.GetHomeGuestArticles(req.Limit.Value, req.Offset.Value);
                Dictionary<string, User> authors = await GetAuthorMap(articles);

                var articlesOutput = articles.Articles
                    .Select(x => _converter(x, authors[x.Author])).ToList();

                var allArticlesCounter = _client.GetGrain<ICounterGrain>(nameof(IArticleGrain));
                var count = await allArticlesCounter.Get();
                return
                (
                    new GetArticlesOutput
                    {
                        Articles = articlesOutput,
                        ArticlesCount = count
                    },
                    Error.None
                );
            }
            catch (Exception ex)
            {
                return (null, new Error("7f9cef3e-ec24-45d9-b59d-f188ed1c6e5b", ex.Message));
            }
        }

        private async Task<Dictionary<string, User>> GetAuthorMap((List<Article> Articles, Error Error) output)
        {
            var authorUsernames = output.Articles.Select(x => x.Author).ToHashSet();
            var tasks = new List<Task<(User, Error)>>();
            foreach (var username in authorUsernames)
            {
                var userGrain = _client.GetGrain<IUserGrain>(username);
                tasks.Add(userGrain.Get());
            }
            var authors = await Task.WhenAll(tasks);
            return authors.ToDictionary(x => x.Item1.Username, y => y.Item1); 
        }
    }
}
