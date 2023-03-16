using Auth.Domain.Entities.Auth;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class GoogleAuthValidator: AbstractValidator<ExternalAuth>
    {
        public GoogleAuthValidator() 
        {
            RuleFor(x => x.Provider).NotNull().NotEmpty()
                .WithMessage("Provider is empty");
            RuleFor(x => x.AccessToken).NotNull().NotEmpty()
                .WithMessage("Google token is empty");
        }
    }
}
