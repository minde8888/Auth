using Auth.Data.Context;
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace tests.Context
{
    public class AppDbContextTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly AppDbContext _dbContext;

        public AppDbContextTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "testDb")
                .Options;

            _dbContext = new AppDbContext(_options);
        }

        [Fact]
        public void DbSet_RefreshToken_ShouldNotBeNull()
        {
            // Act
            var refreshTokenDbSet = _dbContext.RefreshToken;

            // Assert
            Assert.NotNull(refreshTokenDbSet);
        }

        [Fact]
        public void OnModelCreating_ShouldApplyGlobalFilterToUserEntity()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test");
            using var dbContext = new AppDbContext(optionsBuilder.Options);

            // Act
            var user = new ApplicationUser()
            {
                Roles = "Basic",
                Email = "testuser@test.com",
                UserName = "testUser",
                PhoneNumber = "123456789"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            // Assert
            var entityType = dbContext.Model.FindEntityType(typeof(User));
            var isDeletedProperty = entityType?.FindProperty(nameof(User.IsDeleted));
            Assert.NotNull(isDeletedProperty?.GetDefaultValue());
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
