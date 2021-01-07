using Contracts;
using Contracts.Users;
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
        private const string Query =
            @"
                SELECT grainidextensionstring FROM orleansstorage 
                WHERE payloadjson->>'Email' = @email
                    AND graintypestring = 'Grains.Users.UserGrain,Grains.UserGrain'
                LIMIT 1;
            ";
        private const string EmailParam = "@email";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<(string, Error)> GetUsernameByEmail(string email)
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            var id = string.Empty;
            await using (var cmd = new NpgsqlCommand(Query, conn))
            {
                cmd.Parameters.AddWithValue(EmailParam, email);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    id = reader.GetString(0);
                }
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                return (null, new Error("07d9496e-d7fa-436b-8bf5-ce6c37a70c38", $"{email} not found"));
            }
            else
            {
                return (id, Error.None);
            }
        }

        public (string Username, Error Error) GetCurrentUsername()
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
