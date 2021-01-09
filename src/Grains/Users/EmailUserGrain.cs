namespace Grains.Users
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Security;
    using Contracts.Users;
    using Npgsql;
    using Orleans;
    using Orleans.Runtime;

    public class EmailUserGrain : Grain, IEmailUserGrain
    {
        private const string Query =
            @"
                SELECT grainidextensionstring FROM orleansstorage 
                WHERE payloadjson->>'Email' = @email
                    AND graintypestring = 'Grains.Users.UserGrain,Grains.UserGrain'
                LIMIT 1;
            ";
        private const string EmailParam = "@email";

        private string Username { get; set; }

        public async Task<(string, Error)> GetUsername()
        {
            if(!string.IsNullOrWhiteSpace(Username))
            {
                return (Username, Error.None);
            }
            var email = this.GetPrimaryKeyString();
            await using var conn = new NpgsqlConnection(Constants.ConnStr);
            await conn.OpenAsync();

            await using (var cmd = new NpgsqlCommand(Query, conn))
            {
                cmd.Parameters.AddWithValue(EmailParam,email);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    Username = reader.GetString(0);
                }
            }
            if (string.IsNullOrWhiteSpace(Username))
            {
                return (null, new Error("07d9496e-d7fa-436b-8bf5-ce6c37a70c38", $"{email} not found"));
            }
            return (Username, Error.None);
        }

        public async Task<Error> SetUsername(string username)
        {
            Username = username;
            return await Task.FromResult(Error.None);
        }
    }
}
