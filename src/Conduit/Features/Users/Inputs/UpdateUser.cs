﻿namespace Conduit.Features.Users.Inputs
{
    using Conduit.Features.Users.Outputs;
    using Conduit.Infrastructure.Security;
    using Contracts;
    using Contracts.Users;
    using MediatR;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;
    public class UpdateUserWrapper : IRequest<(GetCurrentUserOutput Output, Error Error)>
    {
        public UpdateUser User { get; set; }
    }
    public class UpdateUserHandler :
        IRequestHandler<UpdateUserWrapper, (GetCurrentUserOutput Output, Error Error)>
    {
        private readonly IGrainFactory _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;
        public UpdateUserHandler(IGrainFactory c, IJwtTokenGenerator g, IUserService s)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        public async Task<(GetCurrentUserOutput Output, Error Error)> 
            Handle(UpdateUserWrapper req, CancellationToken ct)
        {
            var userId = _userService.GetCurrentUsername();
            var userGrain = _client.GetGrain<IUserGrain>(userId);
            var error = await userGrain.Update(req.User);
            if (error.Exist())
            {
                return (null, error);
            }

            var result = await userGrain.Get();
            if (result.Error.Exist())
            {
                return (null, error);
            }

            return 
            (
                new GetCurrentUserOutput
                (
                    userGrain.GetPrimaryKeyString(),
                    result.User.Email,
                    result.User.Bio,
                    result.User.Image,
                    await _tokenGenerator.CreateToken(userGrain.GetPrimaryKeyString())
                ), 
                Error.None
            );
        }
    }
}
