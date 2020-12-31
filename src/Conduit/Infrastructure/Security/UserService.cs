using GrainInterfaces;
using GrainInterfaces.Services;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Conduit.Infrastructure.Security
{
    public class UserService : IUserService
    {
        const string connStr = Constants.ConnStr;
        const string table = "orleansstorage";
        const string idColumn = "grainidextensionstring";
        const string emailColumn = "payloadjson->>'Email'";
        const string additionalFilter = "graintypestring = 'Grains.Security.UserGrain,Grains.UserGrain'";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<(string, IError)> GetUsernameByEmail(string email)
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var param = "@p";
            var id = string.Empty;
            //TODO: below line is also too long
            await using (var cmd = new NpgsqlCommand($"SELECT {idColumn} FROM {table} WHERE {emailColumn} = {param} AND {additionalFilter} LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue(param, email);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    id = reader.GetString(0);
                }
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                return (null, new Error("07d9496e-d7fa-436b-8bf5-ce6c37a70c38", $"user of {email} not found"));
            }
            else
            {
                return (id, Error.None);
            }
        }

        public (string, IError) GetCurrentUsername()
        {
            return
            (
                _httpContextAccessor
                .HttpContext.User?
                .Claims?
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
                .Value,
                Error.None
            );
        }
    }
}
