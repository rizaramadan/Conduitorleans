namespace Grains.Users
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Security;
    using Contracts.Users;
    using Orleans;
    using Orleans.Runtime;

    public class UserGrain : Grain, IUserGrain
    {
        public static readonly Error UnregisteredUserLogin =
            new Error("d7a011a1-3f86-4797-b6ef-210b4b041121", "login of unregistered user");

        public static readonly Error UnregisteredUserUpdate =
            new Error("86AEFE6B-D99D-4168-95E1-94E6E330F390", "unregistered user want to update");

        public static readonly Error EmailPasswordMismatch =
            new Error("069e089f-1ff9-49a6-8821-7091ab9fa0a7", "email or password mismatch");

        public static readonly Error UserAlreadyRegistered =
            new Error("e119c62f-c276-4799-a6a0-fbc43da87c2b", "user already registered");



        private readonly IPersistentState<User> _userState;
        private readonly IGrainFactory _factory;

        public UserGrain(   
            [PersistentState("UserGrain", Constants.GrainStorage)] IPersistentState<User> s,
            IGrainFactory f
        )
        {
            _userState = s;
            _factory = f;
        }

        /// <summary>
        /// users are consider registered when having a password
        /// </summary>
        /// <returns>true of use has registered</returns>
        public async Task<(bool, Error)> HasRegistered()
        {
            var result = await Task.FromResult
            (
                _userState.State?.Password?.Length > 0
            );
            return (result, Error.None);
        }
        public async Task<Error> Register(string email, string password)
        {
            var (hasRegistered, error) = await HasRegistered();
            if (error.Exist())
            {
                return error;
            }
            if (hasRegistered)
            {
                return UserAlreadyRegistered;
            }
            try
            {
                _userState.State.Email = email;
                var passwordHasher = _factory.GetGrain<IPasswordHasher>(0);
                _userState.State.Salt = Guid.NewGuid();
                _userState.State.Password =
                    await passwordHasher.Hash(password, _userState.State.Salt.ToByteArray());
                await _userState.WriteStateAsync();
                var emailUserGrain = _factory.GetGrain<IEmailUserGrain>(_userState.State.Email);
                await emailUserGrain.SetUsername(this.GetPrimaryKeyString());
                return Error.None;
            }
            catch (Exception ex)
            {
                return new Error("b1890485-4204-4e1d-84d5-1eab7866dfbc", ex.Message);
            }
        }

        public async Task<Error> Login(string email, string password)
        {
            var (hasRegistered, error) = await HasRegistered();
            if (error.Exist())
            {
                return error;
            }

            if (!hasRegistered)
            {
                return UnregisteredUserLogin;
            }

            try
            {
                var passwordHasher = _factory.GetGrain<IPasswordHasher>(0);
                var challenge = await passwordHasher.Hash(password, _userState.State.Salt.ToByteArray());
                if
                (
                    !_userState.State.Password.SequenceEqual(challenge) ||
                    !_userState.State.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return EmailPasswordMismatch;
                }
                return Error.None;
            }
            catch (Exception ex)
            {
                return new Error("9fca3552-9a5d-45b6-ad96-7a7e115306c2", ex.Message);
            }
        }

        public async Task<Error> Update(UpdateUser user) 
        {
            var (hasRegistered, error) = await HasRegistered();
            if (error.Exist())
            {
                return error;
            }

            if (!hasRegistered)
            {
                return UnregisteredUserUpdate;
            }

            if (!string.IsNullOrEmpty(user.Bio))
            {
                _userState.State.Bio = user.Bio;
            }
            if (!string.IsNullOrEmpty(user.Image))
            {
                _userState.State.Image = user.Image;
            }
            if (!string.IsNullOrWhiteSpace(user.Password)) 
            {
                var passwordHasher = _factory.GetGrain<IPasswordHasher>(0);
                _userState.State.Password =
                    await passwordHasher.Hash(user.Password, _userState.State.Salt.ToByteArray());
            }

            Task<Error> resetTask = null;
            Task<Error> updateTask = null;
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var oldEmailUserGrain = _factory.GetGrain<IEmailUserGrain>(_userState.State.Email);
                resetTask = oldEmailUserGrain.SetUsername(string.Empty);
                var newEmailUserGrain = _factory.GetGrain<IEmailUserGrain>(user.Email);
                updateTask = newEmailUserGrain.SetUsername(this.GetPrimaryKeyString());
                _userState.State.Email = user.Email;
                
            }

            var saveChangesTask = _userState.WriteStateAsync();
            if (resetTask != null && updateTask != null)
            {
                await Task.WhenAll(resetTask, updateTask, saveChangesTask);
            }
            else 
            {
                await saveChangesTask;
            }
            return Error.None;
        }


        public async Task<(string, Error)> GetEmail()
        {
            return await Task.FromResult((_userState.State.Email, Error.None));
        }

        public async Task<(User User, Error Error)> Get()
        {
            var user = new User
            {
                Username = this.GetPrimaryKeyString(),
                Image    = _userState.State.Image,
                Bio      = _userState.State.Bio,
                Email    = _userState.State.Email
            };
            return await Task.FromResult((user, Error.None));
        }
    }
}
