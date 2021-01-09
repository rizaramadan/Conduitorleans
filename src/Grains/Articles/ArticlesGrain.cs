namespace Grains.Articles
{
    using Contracts;
    using Contracts.Articles;
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

        public ArticlesGrain(
            IGrainFactory f
        )
        {
            _factory = f;
        }

        public async Task<(List<Article>, Error)> GetHomeGuestArticles(int limit, int offset)
        {
            try
            {
                var idAuthors = await GetArticlesId(limit, offset);
                var cleanArticles = await GetArticlesData(idAuthors);
                return (cleanArticles, Error.None);
            }
            catch (Exception ex)
            {
                return (null, new Error("6a80f54d-d6a6-4471-99e7-9bb1aec5a323", ex.Message));
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

        private async Task<List<Article>> GetArticlesData(List<(long, string)> idAuthors)
        {
            var tasks = new List<Task<(Article article, Error error)>>(idAuthors.Count);
            foreach (var each in idAuthors)
            {
                var articleGrain = _factory.GetGrain<IArticleGrain>(each.Item1, each.Item2);
                tasks.Add(articleGrain.GetArticle());
            }
            var output = await Task.WhenAll(tasks);
            return output.Where(x => !x.error.Exist()).Select(x => x.article).ToList();
        }
    }
}
