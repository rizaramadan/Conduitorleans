namespace Grains.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Articles;
    using Npgsql;
    using Orleans;

    public class SlugGrain : Grain, ISlugGrain
    {
        private const string Query =
            @"
                SELECT grainidn1, grainidextensionstring
                FROM orleansstorage
                WHERE (payloadjson->>'Slug') = @slug
                  AND graintypestring = 'Grains.Articles.ArticleGrain,Grains.ArticleGrain'
                LIMIT 1;
            ";
        private const string SlugParam = "@slug";

        private long ArticleId { get; set; }
        private string Author { get; set; }
        private readonly IGrainFactory _factory;
        
        public SlugGrain(IGrainFactory f) => _factory = f;

        public override async Task OnActivateAsync()
        {
            var slug = this.GetPrimaryKeyString();
            await using var conn = new NpgsqlConnection(Constants.ConnStr);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(Query, conn);
            cmd.Parameters.AddWithValue(SlugParam, slug);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                ArticleId = reader.GetInt64(0);
                Author = reader.GetString(1);
            }
        }

        public async Task<(Article Article, Error Error)> GetArticle()
        {
            var articleGrain = _factory.GetGrain<IArticleGrain>(ArticleId, Author);
            return await articleGrain.GetArticle();
        }
    }
}
