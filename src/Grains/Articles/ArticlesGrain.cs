namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
    using Contracts.Users;
    using Npgsql;
    using Orleans;
    using Orleans.Concurrency;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [StatelessWorker]
    [Reentrant]
    public class ArticlesGrain : Grain, IArticlesGrain
    {
        private const string Query =
            @"
                SELECT grainidn1, grainidextensionstring 
                FROM orleansstorage 
                WHERE graintypestring = 'Grains.Articles.ArticleGrain,Grains.UserGrain' 
                ORDER BY payloadjson->>'CreatedAt' DESC
                LIMIT @limit
                OFFSET @offset;
            ";
        private const string LimitParam = "@limit";
        private const string OffsetParam = "@offset";

        private readonly IGrainFactory _factory;

        public ArticlesGrain(IGrainFactory f)
        {
            _factory = f;
        }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> GetHomeGuestArticles(int limit, int offset)
        {
            try
            {
                var articlesAndAuthors = await GetArticlesId(limit, offset);
                var cleanArticles = await GetArticlesData(articlesAndAuthors);
                var allArticlesCounter = _factory.GetGrain<ICounterGrain>(nameof(IArticleGrain));
                var count = await allArticlesCounter.Get();
                return (cleanArticles, count, Error.None);
            }
            catch (Exception ex)
            {
                return (null, 0, new Error("6a80f54d-d6a6-4471-99e7-9bb1aec5a323", ex.Message));
            }
        }

        private async Task<List<(long, string)>> GetArticlesId(int limit, int offset)
        {
            await using var conn = new NpgsqlConnection(Constants.ConnStr);
            await conn.OpenAsync();

            var idAuthor = new List<(long, string)>(limit);
            await using (var cmd = new NpgsqlCommand(Query, conn))
            {
                cmd.Parameters.AddWithValue(LimitParam, limit);
                cmd.Parameters.AddWithValue(OffsetParam, offset);
                await cmd.PrepareAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    idAuthor.Add((reader.GetInt64(0), reader.GetString(1)));
                }
            }
            return idAuthor;
        }

        private async Task<List<ArticleUserPair>> GetArticlesData(List<(long, string)> idAuthors)
        {
            
            var articleTasks = new List<Task<(Article article, Error error)>>(idAuthors.Count);
            var authorTasks = new List<Task<(User User, Error Error)>>(idAuthors.Count);
            var authorSet = new HashSet<string>();
            foreach (var each in idAuthors)
            {
                var articleId = each.Item1;
                var authorId = each.Item2;
                var articleGrain = _factory.GetGrain<IArticleGrain>(articleId, authorId);
                articleTasks.Add(articleGrain.GetArticle());
                if (!authorSet.Contains(authorId)) 
                {
                    authorSet.Add(authorId);
                    var userGrain = _factory.GetGrain<IUserGrain>(authorId);
                    authorTasks.Add(userGrain.Get());
                }
            }

            var articles = (await Task.WhenAll(articleTasks)).ToList();
            var authors = (await Task.WhenAll(authorTasks)).ToDictionary(x => x.User.Username, y => y.User);

            return articles.Where(x => !x.error.Exist())
                .Select(x => new ArticleUserPair(x.article, authors[x.article.Author]))
                .ToList();
        }
    }
}
