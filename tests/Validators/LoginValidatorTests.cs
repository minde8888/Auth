using Auth.Domain.Entities;
using Auth.Services.Validators;
using FluentValidation.TestHelper;

namespace tests.Validators
{
    public class LoginValidatorTests
    {
        private readonly LoginValidator validator;

        public LoginValidatorTests()
        {
            validator = new LoginValidator();
        }

        [Fact]
        public void LoginValidator_ValidInput_PassesValidation()
        {
            // Arrange
            var input = new Login
            {
                Email = "test@example.com",
                Password = "Password1!"
            };

            // Act
            var result = validator.TestValidate(input);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void LoginValidator_InvalidInput_FailsValidation()
        {
            // Arrange
            var validator = new LoginValidator();
            var input = new Login
            {
                Email = "invalidEmail",
                Password = ""
            };

            // Act
            var result = validator.TestValidate(input);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.Email).WithErrorMessage("Your email address is not valid");
            result.ShouldHaveValidationErrorFor(r => r.Password).WithErrorMessage("Your password cannot be empty");
        }
    }
}
