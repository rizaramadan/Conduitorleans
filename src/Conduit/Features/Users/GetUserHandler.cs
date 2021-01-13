namespace Conduit.Features.Users
{
    using Conduit.Features.Users.Outputs;
    using Conduit.Infrastructure.Security;
    using Contracts;
    using Contracts.Users;
    using MediatR;
    using Orleans;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetUser : IRequest<(GetCurrentUserOutput, Error Error)> { }

    public class GetUserHandler : IRequestHandler<GetUser, (GetCurrentUserOutput, Error Error)>
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;
        public GetUserHandler(IClusterClient c, IJwtTokenGenerator g, IUserService s)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        public async Task<(GetCurrentUserOutput, Error Error)> Handle(GetUser req, CancellationToken ct)
        {
            var (userId, error) = _userService.GetCurrentUsername();
            if (error.Exist())
            {
                return (null, error);
            }

            var userGrain = _client.GetGrain<IUserGrain>(userId);
            (Contracts.Users.User User, Error Error) = await userGrain.Get();
            if (Error.Exist())
            {
                return (null, Error);
            }

            return
            (
                new GetCurrentUserOutput
                (
                    userGrain.GetPrimaryKeyString(),
                    User.Email,
                    User.Bio,
                    User.Image,
                    await _tokenGenerator.CreateToken(userGrain.GetPrimaryKeyString())
                ),
                Error.None
            );
        }
    }
}
