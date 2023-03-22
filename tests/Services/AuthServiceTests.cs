using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.MapperProfile;
using Auth.Services.WrapServices;
using AutoFixture.Xunit2;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthApi> _authApiMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        private readonly InlineValidator<Signup> _signupValidator;
        private readonly InlineValidator<Login> _loginValidator;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authApiMock = new Mock<IAuthApi>();
            _tokenServiceMock = new Mock<ITokenService>();

            _signupValidator = new InlineValidator<Signup>();
            _loginValidator = new InlineValidator<Login>();

            _authService = new AuthService(
                _authApiMock.Object,
                _tokenServiceMock.Object,
                _signupValidator,
                _loginValidator);
        }


        [Fact]
        public async Task AddUser_GivenUserObject_ReturnsResult()
        {
            // Arrange
            var user = new Signup
            {
                Name = "Test",
                Surname = "Test",
                PhoneNumber = "123456789",
                Email = "test@email.com",
                Password = "Secret!123",
                Roles = "user"
            };
            _loginValidator.RuleFor(x => x.Email).Must(Email => true);
            _authApiMock.Setup(x => x.UserExisitAsync(user.PhoneNumber, user.Email)).Returns(false);
            _authApiMock.Setup(x => x.CreateUserAsync(It.IsAny<ApplicationUser>(), user.Password)).ReturnsAsync(IdentityResult.Success);
            _authApiMock.Setup(x => x.AddRoleAsync(It.IsAny<ApplicationUser>(), user.Roles))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.CreateUserAsync(user);

            // Assert   
            _authApiMock.Verify(x => x.UserExisitAsync(user.PhoneNumber, user.Email), Times.Once);
            _authApiMock.Verify(x => x.CreateUserAsync(It.IsAny<ApplicationUser>(), user.Password), Times.Once);
            _authApiMock.Verify(x => x.AddRoleAsync(It.IsAny<ApplicationUser>(), user.Roles), Times.Once);
            Assert.True(result.Success);
        }
    }
}
