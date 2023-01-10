using Auth.Services.Dtos.Auth;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class RequestTokenValidator : AbstractValidator<RequestToken>
    {
        public RequestTokenValidator()
        {
            RuleFor(x => x.Token).NotNull().NotEmpty()
               .WithMessage("Token is empty");
            RuleFor(x => x.RefreshToken).NotNull().NotEmpty()
                .WithMessage("RefreshToken is empty");
        }
    }
}
