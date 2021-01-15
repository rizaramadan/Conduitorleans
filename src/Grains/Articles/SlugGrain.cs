namespace Grains.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Articles;
    using Npgsql;
    using Orleans;
    using Contracts.Tags;

    public class SlugGrain : Grain, ISlugGrain
    {
        private const string QueryOnActivate =
            @"
                SELECT grainidn1, grainidextensionstring
                FROM orleansstorage
                WHERE (payloadjson->>'Slug') = @slug
                  AND graintypestring = 'Grains.Articles.ArticleGrain,Grains.ArticleGrain'
                LIMIT 1;
            ";

        private const string QueryOnDelete =
            @"
                DELETE FROM orleansstorage
                WHERE (payloadjson->>'Slug') = @slug
                  AND (payloadjson->>'Author') = @author
                  AND graintypestring = 'Grains.Articles.ArticleGrain,Grains.ArticleGrain';
            ";
        private const string SlugParam = "@slug";
        private const string AuthorParam = "@author";

        private long ArticleId { get; set; } = long.MinValue;
        private string Author { get; set; }
        private readonly IGrainFactory _factory;
        
        public SlugGrain(IGrainFactory f) => _factory = f;

        public override async Task OnActivateAsync()
        {
            var slug = this.GetPrimaryKeyString();
            await using var conn = new NpgsqlConnection(Constants.ConnStr);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(QueryOnActivate, conn);
            cmd.Parameters.AddWithValue(SlugParam, slug);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                ArticleId = reader.GetInt64(0);
                Author = reader.GetString(1);
            }
        }

        public async Task<(long ArticleId, string Author)> GetArticleId()
        {
            return await Task.FromResult((ArticleId, Author));
        }

        public async Task<(Article Article, Error Error)> GetArticle()
        {
            if (string.IsNullOrWhiteSpace(Author) || ArticleId == long.MinValue)
            {
                return (null, new Error("D23749EE-EF65-4D44-B40C-0D5B2D28A135", "article not found"));
            }

            var articleGrain = _factory.GetGrain<IArticleGrain>(ArticleId, Author);
            return await articleGrain.Get();
        }

        public async Task<Error> DeleteArticle(string username)
        {
            var slug = this.GetPrimaryKeyString();
            var articleGrain = _factory.GetGrain<IArticleGrain>(ArticleId, Author);
            (Article Article, Error Error) = await articleGrain.Get();
            if (Error.Exist())
            {
                return Error;
            }

            var tasks = new List<Task<Error>>();
            foreach (var tag in Article.TagList ?? Enumerable.Empty<string>())
            {
                var tagGrain = _factory.GetGrain<ITagGrain>(tag);
                tasks.Add(tagGrain.RemoveArticle(ArticleId, Author));
            }
            var counter = _factory.GetGrain<ICounterGrain>(nameof(IArticleGrain));
            tasks.Add(counter.Decreement());
            
            var userArticles = _factory.GetGrain<IUserArticlesGrain>(username);
            tasks.Add(userArticles.RemoveArticle(ArticleId));
            
            var task = DeleteStorage(slug, username);
            tasks.Add(task);
            await Task.WhenAll(task);
            return await task;
        }

        private async Task<Error> DeleteStorage(string slug, string username) 
        {
            await using var conn = new NpgsqlConnection(Constants.ConnStr);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(QueryOnDelete, conn);
            cmd.Parameters.AddWithValue(SlugParam, slug);
            cmd.Parameters.AddWithValue(AuthorParam, username);
            var row = await cmd.ExecuteNonQueryAsync();
            if (row == 1)
            {
                return Error.None;
            }
            return new Error("B20A94FD-71DA-4A8C-B1A1-43B5CAC76503", $"unexpectedly {row} row(s) affected");
        }
    }
}
