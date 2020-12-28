using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrainInterfaces;
using GrainInterfaces.Errors;
using GrainInterfaces.Security;
using GrainInterfaces.Services;
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

        public async Task<IError> Login(string email, string password)
        {
            var (hasRegistered, error) = await HasRegistered();
            if (error.Exist())
                return error;
            if (!hasRegistered)
                return new Error("d7a011a1-3f86-4797-b6ef-210b4b041121", "login of unregistered user");
            var passwordHasher = _factory.GetGrain<IPasswordHasher>(0);
            var challenge = await passwordHasher.Hash(password, _userState.State.Salt.ToByteArray());
            if (_userState.State.Password.SequenceEqual(challenge)
               && _userState.State.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                return Error.None;
            else
                return new Error("069e089f-1ff9-49a6-8821-7091ab9fa0a7", "email or password mismatch");

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
