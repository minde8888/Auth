using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.WrapServices;
using AutoFixture.Xunit2;
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
        public void UselCreate_DidNotGetData_UserExistExceptionn()
        {
            //result 
            Assert.ThrowsAsync<UserExistException>(async () => await _authService.CreateUserAsync(null));
        }

        [Fact]
        public void UserLogin_DidNotGetData_EmployeeNotFoundException()
        {
            //result 
            Assert.ThrowsAsync<UserNotFoundException>(async () => await _authService.AuthAsync(null));
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

        [Theory, AutoData]
        public async Task AuthAsync_GivenUserObject_ReturnsResult( AuthResult auth)
        {
            // Arrange
            var login = new Login()
            {
                Email = "test@tesxt.com",
                Password = "SuperStrongPasword3!"
            };

            _authApiMock.Setup(x => x.AuthUserAsync(login.Email))
                .ReturnsAsync(new ApplicationUser());

            _authApiMock.Setup(x => x.PasswordValidatorAsync(It.IsAny<ApplicationUser>(), login.Password))
                .ReturnsAsync(true);

            _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(auth);

            // Act
            var result = await _authService.AuthAsync(login);

            // Assert   
            _authApiMock.Verify(x => x.AuthUserAsync(login.Email), Times.Once);
            _authApiMock.Verify(x => x.PasswordValidatorAsync(It.IsAny<ApplicationUser>(), login.Password), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            Assert.Equal(auth, result);

        }
    }
}
