using Auth.Domain.Interfaces;
using Auth.Services.WrapServices;
using FluentValidation;
using Moq;
using Auth.Domain.Exceptions;
using Auth.Domain.Entities.Auth;
using AutoFixture.Xunit2;
using Google.Apis.Auth;
using Auth.Services.Services;
using Microsoft.AspNetCore.Identity;

namespace tests.Services
{
    public class ExternAuthServiceTests
    {
        private readonly Mock<IAuthApi> _authApiMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IExternAuth> _externAuthMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly InlineValidator<ExternalAuth> _externAuthValidator;

        private readonly ExternAuthService _externAuthService;

        public ExternAuthServiceTests()
        {
            _authApiMock = new Mock<IAuthApi>();
            _tokenServiceMock = new Mock<ITokenService>();

            _externAuthValidator = new InlineValidator<ExternalAuth>();

            _externAuthMock = new Mock<IExternAuth>();

            _userRepositoryMock = new Mock<IUserRepository>();

            _externAuthService = new ExternAuthService(
                _authApiMock.Object,
                _userRepositoryMock.Object,
                _tokenServiceMock.Object,
                _externAuthValidator,
                _externAuthMock.Object);
        }

        [Fact]
        public async Task GoogleAuth_DidNotGetData_ExternalAuthExceptions()
        {
            // Arrange
            var googleAuth = new ExternalAuth
            {
                Provider = "google.com",
                AccessToken = "accessToken"
            };
            _externAuthMock.Setup(x => x.GoogleJsonValidaror(googleAuth.AccessToken)).ReturnsAsync(new GoogleJsonWebSignature.Payload());

            // Act and Assert
            await Assert.ThrowsAsync<ExternalAuthException>(() => _externAuthService.GoogleAuth(googleAuth));

            // Arrange
            googleAuth.Provider = "invalid_provider";

            // Act and Assert
            await Assert.ThrowsAsync<ExternalAuthException>(() => _externAuthService.GoogleAuth(googleAuth));

            // Arrange
            _externAuthMock.Setup(x => x.GoogleJsonValidaror(googleAuth.AccessToken)).ReturnsAsync(new GoogleJsonWebSignature.Payload { Email = "test@test.com" });
            _userRepositoryMock.Setup(x => x.GetUserByEmail("test@test.com")).ReturnsAsync(new ApplicationUser());

            // Act and Assert
            await Assert.ThrowsAsync<ExternalAuthException>(() => _externAuthService.GoogleAuth(googleAuth));

            // Arrange
            var user = new ApplicationUser();
            _userRepositoryMock.Setup(x => x.GetUserByEmail("test@test.com")).ReturnsAsync(user);
            _authApiMock.Setup(x => x.CreateBasicUser(user)).ReturnsAsync(new IdentityResult());
            _authApiMock.Setup(x => x.AddRoleAsync(user, user.Roles)).ThrowsAsync(new Exception());

            // Act and Assert
            await Assert.ThrowsAsync<ExternalAuthException>(() => _externAuthService.GoogleAuth(googleAuth));
        }


        [Theory, AutoData]
        public async Task GoogleAuthSigup_GivenUserObject_ReturnsResult(
            ExternalAuth googleAuth,
            GoogleJsonWebSignature.Payload payload,
            AuthResult auth)
        {
            // Arrange
            var user = new ApplicationUser
            {
                Roles = "Basic",
                Email = "test@test.com",
                UserName = "Name"
            };
            googleAuth.Provider = "google.com";
            payload.Email = user.Email;

            _externAuthMock.Setup(x => x.GoogleJsonValidaror(googleAuth.AccessToken))
                .ReturnsAsync(payload);

            _userRepositoryMock.Setup(x => x.GetUserByEmail(null))
                .ReturnsAsync(user);

            _authApiMock.Setup(x => x.CreateBasicUser(user)).ReturnsAsync(IdentityResult.Success);
            _authApiMock.Setup(x => x.AddRoleAsync(user, user.Roles));

            _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(auth);

            // Act
            var result = await _externAuthService.GoogleAuth(googleAuth);

            // Assert   
            _externAuthMock.Verify(x => x.GoogleJsonValidaror(googleAuth.AccessToken), Times.Once);
            _userRepositoryMock.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Once);
            Assert.Equal(auth, result);
        }

    }
}
