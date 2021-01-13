using Contracts;
using Contracts.Follows;
using Contracts.Users;
using MediatR;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Features.Follows
{
    public class Follow : IRequest<(Profile Profile, Error Error)>
    {
        public string Username { get; }

        public Follow(string username)
        {
            this.Username = username;
        }
    }

    public class FollowHandler : BaseFollow, IRequestHandler<Unfollow, (Profile Profile, Error Error)>
    {
        public FollowHandler(IClusterClient c, IUserService u) : base(c, u) { }

        public async Task<(Profile Profile, Error Error)> Handle(Unfollow req, CancellationToken ct)
        {
            return await BaseHandle(req.Username, ct);
        }

        protected override Task<Error> Process(string username, IUserFollowingGrain followingGrain)
        {
            return followingGrain.Follow(username);
        }
    }
}
