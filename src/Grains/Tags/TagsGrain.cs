namespace Grains.Tags
{
    using Contracts;
    using Contracts.Tags;
    using Npgsql;
    using Orleans;
    using Orleans.Concurrency;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    [StatelessWorker]
    [Reentrant]
    public class TagsGrain : Grain, ITagsGrain
    {
        private const string Query =
            @"
                SELECT grainidextensionstring 
                FROM orleansstorage 
                WHERE graintypestring = 'Grains.Tags.TagGrain,Grains.TagGrain';
            ";

        public async Task<(List<string> Tags, Error Error)> GetTags()
        {
            try
            {
                await using var conn = new NpgsqlConnection(Constants.ConnStr);
                await conn.OpenAsync();

                var tags = new List<string>();
                await using (var cmd = new NpgsqlCommand(Query, conn))
                {
                    await cmd.PrepareAsync();
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tags.Add(reader.GetString(0));
                    }
                }
                return (tags, Error.None);
            }
            catch (Exception ex)
            {
                return (null, new Error("4e349245-5009-41f5-bd22-2a596123f778", ex.Message));
            }
        }
    }
}
