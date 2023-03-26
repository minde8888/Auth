using Auth.Domain.Entities.Auth;
using Auth.Services.WrapServices;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace tests.WrapServices
{
    public class TokenApiTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ITokenApi _tokenApi;

        public TokenApiTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _tokenApi = new TokenApi(_userManagerMock.Object);
        }

        [Fact]
        public async Task RolesAsync_ReturnsUserRoles()
        {
            // Arrange
            var user = new ApplicationUser();
            var roles = new List<string> { "Role1", "Role2" };
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _tokenApi.RolesAsync(user);

            // Assert
            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task FindUserIdAsync_ReturnsUser()
        {
            // Arrange
            var id = Guid.NewGuid(); 
            var user = new ApplicationUser { Id = id };
            _userManagerMock.Setup(x => x.FindByIdAsync(id.ToString())).ReturnsAsync(user);

            // Act
            var result = await _tokenApi.FindUserIdAsync(id.ToString());

            // Assert
            Assert.Equal(user, result);
        }

        [Fact]
        public async Task FindUserLoginAsync_ReturnsUser()
        {
            // Arrange
            var loginProvider = "Google";
            var providerKey = "123456";
            var user = new ApplicationUser();
            _userManagerMock.Setup(x => x.FindByLoginAsync(loginProvider, providerKey)).ReturnsAsync(user);

            // Act
            var result = await _tokenApi.FindUserLoginAsync(loginProvider, providerKey);

            // Assert
            Assert.Equal(user, result);
        }
    }
}
