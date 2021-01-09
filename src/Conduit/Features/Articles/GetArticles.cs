namespace Conduit.Features.Articles.Inputs
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
        private readonly IClusterClient _client;
        private static readonly Func<Article, GetArticleOutput> _converter = x =>
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
                Author         = new ArticleAuthor { Username = x.Author },
                Favorited      = x.Favorited,
                FavoritesCount = x.FavoritesCount
            };
            return output;
        };

        public GetArticlesHandler(IClusterClient c) => _client = c;

        public async Task<(GetArticlesOutput Output, Error Error)> 
            Handle(GetArticlesInput req, CancellationToken ct)
        {
            var articlesGrain = _client.GetGrain<IArticlesGrain>(0);
            var output = await articlesGrain.GetHomeGuestArticles(req.Limit.Value, req.Offset.Value);
            var articles = output.Articles.Select(x => _converter(x)).ToList();
            return 
            (
                new GetArticlesOutput
                {
                    Articles = articles,
                    ArticlesCount = output.Articles.Count
                }, 
                Error.None
            );
        }
    }
}
