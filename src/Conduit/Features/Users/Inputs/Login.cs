using Conduit.Features.Users.Outputs;
using Conduit.Infrastructure.Security;
using Contracts;
using Contracts.Users;
using FluentValidation;
using MediatR;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Features.Users.Inputs
{
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginValidator : AbstractValidator<LoginWrapper> 
    {
        public LoginValidator() 
        {
            RuleFor(lw => lw.User).NotNull();
            RuleFor(lw => lw.User.Email).NotEmpty();
            RuleFor(lw => lw.User.Password).NotEmpty();
        }
    }
    public class LoginWrapper : IRequest<(LoginUserOutput Output, Error Error)>
    {
        public Login User { get; set; }
    }

    public class LoginWrapperHandler : 
        IRequestHandler<LoginWrapper, (LoginUserOutput Output, Error Error)>
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public LoginWrapperHandler(IClusterClient c, IJwtTokenGenerator g)
        {
            _client = c;
            _tokenGenerator = g;
        }

        public async Task<(LoginUserOutput Output, Error Error)> Handle(LoginWrapper l, CancellationToken ct)
        {
            var emailUserGrain = _client.GetGrain<IEmailUserGrain>(l.User.Email);
            var (userId, error) = await emailUserGrain.GetUsername();
            if (error.Exist())
            {
                return (null, error);
            }

            var userGrain = _client.GetGrain<IUserGrain>(userId);
            var errorLogin = await userGrain.Login(l.User.Email, l.User.Password);
            if (errorLogin.Exist())
            {
                return (null, errorLogin);
            }

            var user = await userGrain.Get();

            return 
            (
                new LoginUserOutput
                (
                    userGrain.GetPrimaryKeyString(),
                    l.User.Email,
                    //TODO: update bio feature
                    user.User.Bio,
                    //TODO: update image feature
                    user.User.Image,
                    await _tokenGenerator.CreateToken(userGrain.GetPrimaryKeyString())
                ),
                Error.None
            );
        }
    }
}
