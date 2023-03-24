using Auth.Domain.Entities;
using Auth.Services.Validators;

namespace tests.Validators
{
    public class SignupValidatorTests
    {
        private readonly SignupValidator _validator;

        public SignupValidatorTests()
        {
            _validator = new SignupValidator();
        }

        [Fact]
        public void SignupValidator_ValidInput_ReturnsNoErrors()
        {
            // Arrange
            var signup = new Signup
            {
                Name = "John",
                Surname = "Doe",
                PhoneNumber = "1234567890",
                Email = "john.doe@example.com",
                Password = "Abcd1234*",
                Roles = "User"
            };

            // Act
            var result = _validator.Validate(signup);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void SignupValidator_InvalidInput_ReturnsValidationErrors()
        {
            // Arrange
            var validator = new SignupValidator();
            var signup = new Signup
            {
                Name = "",
                Surname = "",
                PhoneNumber = "123",
                Email = "notValidemail",
                Password = "password",
                Roles = ""
            };

            // Act
            var result = validator.Validate(signup);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(11, result.Errors.Count);
            Assert.Contains(result.Errors, x => x.PropertyName == "Name" && x.ErrorMessage.Contains("required"));
            Assert.Contains(result.Errors, x => x.PropertyName == "Name" && x.ErrorMessage.Contains("between 2 and 20"));
            Assert.Contains(result.Errors, x => x.PropertyName == "Surname" && x.ErrorMessage.Contains("required"));
            Assert.Contains(result.Errors, x => x.PropertyName == "Surname" && x.ErrorMessage.Contains("between 2 and 20"));
            Assert.Contains(result.Errors, x => x.PropertyName == "PhoneNumber" && x.ErrorMessage.Contains("10"));
            Assert.Contains(result.Errors, x => x.PropertyName == "Email" && x.ErrorMessage.Contains("valid"));
            Assert.Contains(result.Errors, x => x.PropertyName == "Password" && x.ErrorMessage.Contains("uppercase"));
        }

    }
}
