using Auth.Services.Dtos.Auth;
using Auth.Services.Validators;

namespace tests.Validators
{
    public class RequestTokenValidatorTests
    {
        [Fact]
        public void RequestTokenValidator_ValidInput_ReturnsNoErrors()
        {
            // Arrange
            var validator = new RequestTokenValidator();
            var input = new RequestToken
            {
                Token = "sample token",
                RefreshToken = "sample refresh token"
            };

            // Act
            var result = validator.Validate(input);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void RequestTokenValidator_NullInput_ReturnsValidationError()
        {
            // Arrange
            var validator = new RequestTokenValidator();
            var input = new RequestToken { Token = null, RefreshToken = null };

            // Act
            var result = validator.Validate(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count);
            Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("Token cannot be empty"));
        }


        [Fact]
        public void RequestTokenValidator_InvalidInput_ReturnsValidationError()
        {
            // Arrange
            var validator = new RequestTokenValidator();
            var request = new RequestToken { Token = null, RefreshToken = "" };

            // Act
            var result = validator.Validate(request);

            // Assert
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(result.Errors, x => x.PropertyName == "Token" && x.ErrorMessage.Contains("Token cannot be empty"));
            Assert.Contains(result.Errors, x => x.PropertyName == "RefreshToken" && x.ErrorMessage == "RefreshToken cannot be empty");
        }

    }
}
