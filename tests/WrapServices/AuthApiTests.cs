using Auth.Domain.Entities.Auth;
using Auth.Services.WrapServices;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace tests.WrapServices
{
    public class AuthApiTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly IAuthApi _authApi;

        public AuthApiTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _authApi = new AuthApi(_userManagerMock.Object);
        }

        [Fact]
        public void UserExistAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var phoneNumber = "555-1234";
            var email = "test@example.com";

            var user = new ApplicationUser
            {
                PhoneNumber = phoneNumber,
                Email = email
            };

            _userManagerMock.Setup(x => x.Users)
                .Returns(new List<ApplicationUser> { user }.AsQueryable());

            // Act
            var result = _authApi.UserExisitAsync(phoneNumber, email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnIdentityResult()
        {
            // Arrange
            var user = new ApplicationUser();
            var password = "P@ssword123";

            var expected = IdentityResult.Success;

            _userManagerMock.Setup(x => x.CreateAsync(user, password))
                .ReturnsAsync(expected);

            // Act
            var result = await _authApi.CreateUserAsync(user, password);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CreateBasicUser_ShouldReturnIdentityResult()
        {
            // Arrange
            var user = new ApplicationUser();

            var expected = IdentityResult.Success;

            _userManagerMock.Setup(x => x.CreateAsync(user))
                .ReturnsAsync(expected);

            // Act
            var result = await _authApi.CreateBasicUser(user);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task AddRoleAsync_ShouldCallUserManager()
        {
            // Arrange
            var user = new ApplicationUser();
            var role = "Admin";

            // Act
            await _authApi.AddRoleAsync(user, role);

            // Assert
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, role), Times.Once);
        }

        [Fact]
        public async Task AuthUserAsync_ShouldReturnApplicationUser()
        {
            // Arrange
            var email = "test@example.com";

            var user = new ApplicationUser
            {
                Email = email
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _authApi.AuthUserAsync(email);

            // Assert
            Assert.Equal(user, result);
        }

        [Fact]
        public async Task PasswordValidatorAsync_ShouldReturnTrue()
        {
            // Arrange
            var user = new ApplicationUser();
            var password = "P@ssword123";

            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            // Act
            var result = await _authApi.PasswordValidatorAsync(user, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RolesAsync_ShouldReturnRoles()
        {
            // Arrange
            var user = new ApplicationUser { Email = "user@example.com" };
            var roles = new List<string> { "Role1", "Role2" };
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act
            var result = await _authApi.RolesAsync(user);

            // Assert
            Assert.Equal(roles, result);
        }
    }
}
