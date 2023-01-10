using Auth.Domain.Entities.Auth;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class GoogleAuthValidator: AbstractValidator<GoogleAuth>
    {
        public GoogleAuthValidator() 
        {
            RuleFor(x => x.Provider).NotNull().NotEmpty()
                .WithMessage("Provider is empty");
            RuleFor(x => x.IdToken).NotNull().NotEmpty()
                .WithMessage("Google token is empty");
        }
    }
}
