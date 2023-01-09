using Auth.Domain.Entities;
using FluentValidation;

namespace Auth.Services.Validators
{
    public class LoginValidator : AbstractValidator<Login>
    {
        public LoginValidator() 
        {
            RuleFor(s => s.Email).NotEmpty().WithMessage("Email address is required")
             .EmailAddress().WithMessage("Your email address is not valid");
            RuleFor(p => p.Password).NotEmpty().WithMessage("Your password cannot be empty")
                .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");
        }
    }
}
