namespace Conduit.Features.Articles
{
    using Conduit.Features.Articles.Outputs;
    using Contracts;
    using Contracts.Articles;
    using MediatR;
    using Orleans;
    using System;
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
                Author         = new ArticleAuthor 
                { 
                    Username = x.User.Username, 
                    Bio      = x.User.Bio, 
                    Image    = x.User.Image 
                }
            };
            return output;
        };

        private readonly IClusterClient _client;
        private readonly IMediator _mediator;

        public GetArticlesHandler(IClusterClient c, IMediator m)
        {
            _client = c;
            _mediator = m;
        }

        public async Task<(GetArticlesOutput Output, Error Error)> 
            Handle(GetArticlesInput req, CancellationToken ct)
        {
            try
            {
                var articlesGrain = _client.GetGrain<IArticlesGrain>(0);
                var result = await articlesGrain.GetHomeGuestArticles(req.Limit.Value, req.Offset.Value);
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
    }
}
