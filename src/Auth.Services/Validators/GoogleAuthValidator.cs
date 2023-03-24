using Auth.Domain.Entities.Auth;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class GoogleAuthValidator: AbstractValidator<ExternalAuth>
    {
        public GoogleAuthValidator()
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is empty")
                .NotNull().WithMessage("Provider is required");

            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Google token is empty")
                .NotNull().WithMessage("Google token is required");
        }

    }
}
