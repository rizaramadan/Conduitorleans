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
        private readonly IGrainFactory _factory;

        public UserGrain(
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<UserState> s,
            ILogger<UserGrain> l,
            IGrainFactory f

        )
        {
            _userState = s;
            _l = l;
            _factory = f;
        }

        /// <summary>
        /// users are consider registered when having a password
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, IError)> HasRegistered()
        {
            var result = await Task.FromResult(_userState.State.Password != null && _userState.State.Password.Length > 0);
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
            var passwordHasher = _factory.GetGrain<IPasswordHasher>(0);
            _userState.State.Salt = Guid.NewGuid();
            _userState.State.Password = await passwordHasher.Hash(password, _userState.State.Salt.ToByteArray());
            await _userState.WriteStateAsync();
            return Error.None;
        }
    }
}
