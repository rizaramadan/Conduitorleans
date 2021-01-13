namespace Conduit.Features.Users.Inputs
{
    using Conduit.Features.Users.Outputs;
    using Conduit.Infrastructure.Security;
    using Contracts;
    using Contracts.Users;
    using FluentValidation;
    using MediatR;
    using Orleans;
    using System.Threading;
    using System.Threading.Tasks;

    public class Register
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterWrapperValidator : AbstractValidator<RegisterWrapper>
    {
        public RegisterWrapperValidator()
        {
            RuleFor(r => r.User).NotNull();
            RuleFor(r => r.User.Email).NotEmpty();
            RuleFor(r => r.User.Username).NotEmpty();
            RuleFor(r => r.User.Password).NotEmpty();
        }
    }

    public class RegisterWrapper : IRequest<(RegisterUserOutput Output, Error Error)>
    {
        public Register User { get; set; }
    }



    public class RegisterWrapperHandler :
        IRequestHandler<RegisterWrapper, (RegisterUserOutput Output, Error Error)>
    {
        private readonly IClusterClient _client;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IUserService _userService;

        public RegisterWrapperHandler(IClusterClient c, IJwtTokenGenerator g, IUserService s)
        {
            _client = c;
            _tokenGenerator = g;
            _userService = s;
        }

        public async Task<(RegisterUserOutput Output, Error Error)> Handle(RegisterWrapper r, CancellationToken ct)
        {
            var userGrain = _client.GetGrain<IUserGrain>(r.User.Username);
            var error = await userGrain.Register(r.User.Email, r.User.Password);
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
                new RegisterUserOutput
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
