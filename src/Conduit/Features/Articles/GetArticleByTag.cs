namespace Conduit.Features.Articles
{
    using Conduit.Features.Articles.Outputs;
    using Contracts;
    using Contracts.Articles;
    using Contracts.Tags;
    using MediatR;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetArticleByTag : IRequest<(List<Article> Articles, Error Error)>
    {
        public GetArticlesInput Input { get; }
        public GetArticleByTag(GetArticlesInput i) => Input = i;
    }

    public class GetArticlesByTagHandler : IRequestHandler<GetArticleByTag,
                                           (List<Article> Articles, Error Error)>
    {
        private readonly IClusterClient _client;

        public GetArticlesByTagHandler(IClusterClient c)
        {
            _client = c;
        }

        public async Task<(List<Article> Articles, Error Error)> 
            Handle(GetArticleByTag req, CancellationToken ct)
        {
            var tagGrain = _client.GetGrain<ITagGrain>(req.Input.Tag);
            (List<(long ArticleId, string Author)> ArticleIds, Error Error) tag = await tagGrain.GetArticles();
            var latest = tag.ArticleIds.OrderByDescending(x => x)
                .Skip(req.Input.Offset.Value)
                .Take(req.Input.Limit.Value)
                .ToList();

            var articleTasks = new List<Task<(Article Article, Error Error)>>(latest.Count);
            foreach (var each in latest) 
            {
                var articleGrain = _client.GetGrain<IArticleGrain>(each.ArticleId, each.Author);
                articleTasks.Add(articleGrain.GetArticle());
            }
            var result = (await Task.WhenAll(articleTasks)).ToList();
            return (result.Select(x => x.Article).ToList(), Error.None);
        }
    }
}
