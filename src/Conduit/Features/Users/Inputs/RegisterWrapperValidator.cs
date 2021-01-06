using FluentValidation;

namespace Conduit.Features.Users.Inputs
{
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
}
