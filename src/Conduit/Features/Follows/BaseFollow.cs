namespace Conduit.Features.Follows
{
    using Contracts;
    using Contracts.Follows;
    using Contracts.Users;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class BaseFollow
    {
        protected readonly IClusterClient _client;
        protected readonly IUserService _userService;
        public BaseFollow(IClusterClient c, IUserService u)
        {
            _client = c;
            _userService = u;
        }

        public async Task<(Profile Profile, Error Error)> BaseHandle(string username, CancellationToken ct)
        {
            (string Username, Error Error) me = _userService.GetCurrentUsername();
            var followingGrain = _client.GetGrain<IUserFollowingGrain>(me.Username);
            Task<Error> followingTask = Process(username, followingGrain);
            var getUserTask = _client.GetGrain<IUserGrain>(username).Get();
            await Task.WhenAll(followingTask, getUserTask);

            var followingError = await followingTask;
            if (followingError.Exist())
            {
                return (null, followingError);
            }

            (User User, Error Error) = await getUserTask;
            if (followingError.Exist())
            {
                return (null, followingError);
            }
            return
            (
                new Profile
                {
                    Username = User.Username,
                    Bio = User.Bio,
                    Image = User.Image,
                    Following = await followingGrain.IsFollow(username)
                },
                Error.None
            );
        }

        protected abstract Task<Error> Process(string username, IUserFollowingGrain followingGrain);
    }
}
