using System.Collections.Generic;

namespace Grains.Follows
{
    using Contracts;
    using Contracts.Follows;
    using Orleans;
    using Orleans.Runtime;
    using System.Collections.Immutable;
    using System.Threading.Tasks;

    using PersistenceState = 
        Orleans.Runtime.IPersistentState<HashSet<string>>;

    public class UserFollowingGrain : Grain, IUserFollowingGrain
    {
        private readonly PersistenceState _following;
        private readonly IGrainFactory _factory;

        public UserFollowingGrain(
            [PersistentState(nameof(UserFollowingGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _following = s;
            _factory = f;
        }

        public async Task<bool> IsFollow(string username)
        {
            return await Task.FromResult(_following.State.Contains(username));
        }

        public async Task<Error> Follow(string username)
        {
            if (_following.State == null)
            {
                _following.State = new HashSet<string>
                {
                    username
                };
            }
            else 
            {
                _following.State.Add(username);
            }

            var followers = _factory.GetGrain<IUserFollowersGrain>(username);
            var followersTask = followers.AddFollower(this.GetPrimaryKeyString());
            await Task.WhenAll(_following.WriteStateAsync(), followersTask);
            return Error.None;
        }

        public async Task<Error> Unfollow(string username)
        {
            if (_following.State != null && _following.State.Contains(username))
            {
                _following.State.Remove(username);
            }

            var followers = _factory.GetGrain<IUserFollowersGrain>(username);
            var followersTask = followers.RemoveFollower(this.GetPrimaryKeyString());
            await Task.WhenAll(_following.WriteStateAsync(), followersTask);
            return Error.None;
        }

        public async Task<(HashSet<string> Following, Error Error)> Get()
        {
            return await Task.FromResult((_following.State, Error.None));
        }
    }
}
