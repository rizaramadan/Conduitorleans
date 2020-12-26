using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrainInterfaces;
using GrainInterfaces.Errors;
using GrainInterfaces.Security;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Grains.Security
{
    public class UserGrain : Grain, IUserGrain
    {
        
        private static readonly HMACSHA512 x = new HMACSHA512(Encoding.UTF8.GetBytes("Conduitorleans"));
        private readonly IPersistentState<UserState> _userState;
        private readonly ILogger<UserGrain> _l;
        //private readonly IGrainFactory _factory;

        public UserGrain(
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<UserState> s,
            ILogger<UserGrain> l,
            IGrainFactory f

        )
        {
            _userState = s;
            _l = l;
            //_factory = f;
        }

        /// <summary>
        /// users are consider registered when having a password
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, IError)> HasRegistered()
        {
            var result = await Task.FromResult(!string.IsNullOrWhiteSpace(_userState.State.Password));
            return (result, Error.None);
        }

        public async Task<IError> Register(string email, string password)
        {
            var (hasRegistered, error) = await HasRegistered();
            if (error.Exist())
                return error;
            if (hasRegistered)
                return new UserRegisteredError();
            _userState.State.Email = email;
            //var passwordHasher = _factory.GetGrain<IPasswordHasher>(string.Empty);
            _userState.State.Salt = Guid.NewGuid();
            _userState.State.Password = Convert.ToBase64String(await Hash(password, _userState.State.Salt.ToByteArray()));
            await _userState.WriteStateAsync();
            return Error.None;
        }

        private async Task<byte[]> Hash(string password, byte[] salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password);

            var allBytes = new byte[bytes.Length + salt.Length];
            Buffer.BlockCopy(bytes, 0, allBytes, 0, bytes.Length);
            Buffer.BlockCopy(salt, 0, allBytes, bytes.Length, salt.Length);

            return await Task.FromResult(x.ComputeHash(allBytes));
        }
    }

    
}
