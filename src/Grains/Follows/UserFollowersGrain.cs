﻿namespace Grains.Follows
{
    using Contracts;
    using Contracts.Follows;
    using Orleans;
    using Orleans.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using System.Threading.Tasks;
    using PersistenceState =
        Orleans.Runtime.IPersistentState<System.Collections.Generic.HashSet<string>>;
    public class UserFollowersGrain : Grain, IUserFollowersGrain
    {
        private readonly PersistenceState _followers;
        private readonly IGrainFactory _factory;

        public UserFollowersGrain(
            [PersistentState(nameof(UserFollowingGrain), Constants.GrainStorage)] PersistenceState s,
            IGrainFactory f
        )
        {
            _followers = s;
            _factory = f;
        }

        public async Task<Error> AddFollower(string username)
        {
            if (_followers.State == null)
            {
                _followers.State = new HashSet<string>
                {
                    username
                };
            }
            else
            {
                _followers.State.Add(username);
            }

            await _followers.WriteStateAsync();
            return Error.None;
        }

        public async Task<Error> RemoveFollower(string username)
        {
            if (_followers.State != null && _followers.State.Contains(username))
            {
                _followers.State.Remove(username);
            }

            await _followers.WriteStateAsync();
            return Error.None;
        }
    }
}
