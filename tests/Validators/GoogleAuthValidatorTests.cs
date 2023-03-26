using Auth.Domain.Entities.Auth;
using Auth.Services.Validators;
using FluentValidation.TestHelper;

namespace tests.Validators
{
    public class GoogleAuthValidatorTests
    {
        private readonly GoogleAuthValidator _validator;

        public GoogleAuthValidatorTests()
        {
            _validator = new GoogleAuthValidator();
        }

        [Fact]
        public void GoogleAuthValidator_ValidInput_PassesValidation()
        {
            var model = new ExternalAuth
            {
                Provider = "Google",
                AccessToken = "abc123"
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.Provider);
            result.ShouldNotHaveValidationErrorFor(x => x.AccessToken);
        }

        [Fact]
        public void GoogleAuthValidator_NullProvider_FailsValidation()
        {
            var model = new ExternalAuth
            {
                Provider = null,
                AccessToken = "abc123"
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Provider)
                .WithErrorMessage("Provider is empty");
            result.ShouldNotHaveValidationErrorFor(x => x.AccessToken);
        }

        [Fact]
        public void GoogleAuthValidator_EmptyProvider_FailsValidation()
        {
            var model = new ExternalAuth
            {
                Provider = "",
                AccessToken = "abc123"
            };

            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Provider)
                .WithErrorMessage("Provider is empty");
            result.ShouldNotHaveValidationErrorFor(x => x.AccessToken);
        }

        [Fact]
        public void GoogleAuthValidator_NullAccessToken_FailsValidation()
        {
            var model = new ExternalAuth
            {
                Provider = "Google",
                AccessToken = null
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.Provider);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken)
                .WithErrorMessage("Google token is empty");
        }

        [Fact]
        public void GoogleAuthValidator_EmptyAccessToken_FailsValidation()
        {
            var model = new ExternalAuth
            {
                Provider = "Google",
                AccessToken = ""
            };

            var result = _validator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.Provider);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken)
                .WithErrorMessage("Google token is empty");
        }
    }
}
