using Auth.Data.Context;
using Auth.Data.Configuration;
using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Auth.Tests.Context
{
    public class ContextSeedTests
    {
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;

        public ContextSeedTests()
        {
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(Mock.Of<IRoleStore<ApplicationRole>>(), null, null, null, null);
            _userManager = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        }
        [Fact]
        public async Task SeedEssentialsAsync_ShouldCreateDefaultRoles()
        {
            // Arrange
            var rolesToCreate = new[] { Authorization.Roles.SuperAdmin, Authorization.Roles.Admin, Authorization.Roles.Moderator, Authorization.Roles.Basic };
            var createdRoles = new List<ApplicationRole>();
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
                .Callback<ApplicationRole>(r => createdRoles.Add(r))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await ContextSeed.SeedEssentialsAsync(_userManager.Object, _roleManagerMock.Object);

            // Assert
            foreach (var role in rolesToCreate)
            {
                var roleExists = createdRoles.Any(r => r.Name == role.ToString());
                Assert.True(roleExists, $"Role '{role.ToString()}' was not created.");
            }
        }

        [Fact]
        public async Task SeedEssentialsAsync_ShouldCreateDefaultUser()
        {
            // Arrange
            var defaultUser = new ApplicationUser
            {
                UserName = Authorization.default_username,
                Email = Authorization.default_email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            await ContextSeed.SeedEssentialsAsync(_userManager.Object, _roleManagerMock.Object);

            // Assert
            _userManager.Verify(u => u.CreateAsync(It.Is<ApplicationUser>(user => user.UserName == defaultUser.UserName &&
                                                                                  user.Email == defaultUser.Email &&
                                                                                  user.EmailConfirmed == defaultUser.EmailConfirmed &&
                                                                                  user.PhoneNumberConfirmed == defaultUser.PhoneNumberConfirmed),
                                                 Authorization.default_password), Times.Once);
            _userManager.Verify(u => u.AddToRoleAsync(It.Is<ApplicationUser>(user => user.UserName == defaultUser.UserName), Authorization.default_role.ToString()), Times.Once);
        }
    }
}
