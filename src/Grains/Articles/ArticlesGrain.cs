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
    public class ArticlesGrain : BaseArticleGrain, IArticlesGrain
    {
        private const string Query =
            @"
                SELECT grainidn1, grainidextensionstring 
                FROM orleansstorage 
                WHERE graintypestring = 'Grains.Articles.ArticleGrain,Grains.ArticleGrain' 
                ORDER BY payloadjson->>'CreatedAt' DESC
                LIMIT @limit
                OFFSET @offset;
            ";
        private const string LimitParam = "@limit";
        private const string OffsetParam = "@offset";


        public ArticlesGrain(IGrainFactory f) : base(f) { }

        public async Task<(List<ArticleUserPair> Articles, ulong Count, Error Error)> 
            GetHomeGuestArticles(string currentUser, int limit, int offset)
        {
            try
            {
                var articlesAndAuthors = await GetArticlesId(limit, offset);
                var cleanArticles = await GetArticlesData(currentUser, articlesAndAuthors);
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
    }
}
