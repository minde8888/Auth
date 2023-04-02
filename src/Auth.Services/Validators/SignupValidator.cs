using Auth.Domain.Entities;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class SignupValidator : AbstractValidator<Signup>
    {
        public SignupValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").Length(2, 20);
            RuleFor(x => x.Surname).NotEmpty().WithMessage("Surname is required").Length(2, 20);
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required").Length(8);
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email address is required").EmailAddress().WithMessage("Your email address is not valid");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required")
                .Length(8, 16).WithMessage("Password must be between 8 and 16 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[!\\?\\*\\.]").WithMessage("Password must contain at least one of the following symbols: !?*.");
            RuleFor(x => x.Roles).NotEmpty().WithMessage("Role is required").Length(2, 20);
        }

    }
}
