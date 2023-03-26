using Auth.Data.Context;
using Auth.Data.Repositories;
using Auth.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace tests.Repository
{
    public class TokenRepositoryTests
    {
        [Fact]
        public async Task AddTokenAsync_ShouldAddTokenToContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "AddTokenAsync_ShouldAddTokenToContext")
                .Options;

            using var context = new AppDbContext(options);
            var repository = new TokenRepository(context);
            var token = new RefreshToken
            {
                Token = "abc123",
                UserId = Guid.NewGuid()
            };

            // Act
            await repository.AddTokenAsync(token);

            // Assert
            Assert.Equal(1, await context.RefreshToken.CountAsync());
            var result = await context.RefreshToken.FirstAsync();
            Assert.Equal(token.Token, result.Token);
            Assert.Equal(token.UserId, result.UserId);
        }

        [Fact]
        public async Task GetToken_ShouldReturnNullIfTokenNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "GetToken_ShouldReturnNullIfTokenNotFound")
                .Options;

            using var context = new AppDbContext(options);
            var repository = new TokenRepository(context);

            // Act
            var result = await repository.GetToken("abc123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetToken_ShouldReturnTokenIfFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "GetToken_ShouldReturnTokenIfFound")
                .Options;

            using var context = new AppDbContext(options);
            var repository = new TokenRepository(context);
            var token = new RefreshToken
            {
                Token = "abc123",
                UserId = Guid.NewGuid()
            };
            await context.RefreshToken.AddAsync(token);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetToken(token.Token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(token.Token, result.Token);
            Assert.Equal(token.UserId, result.UserId);
        }

        [Fact]
        public async Task Update_ShouldUpdateTokenInContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "Update_ShouldUpdateTokenInContext")
                .Options;

            using var context = new AppDbContext(options);
            var repository = new TokenRepository(context);
            var token = new RefreshToken
            {
                Token = "abc123",
                UserId = Guid.NewGuid()
            };
            await context.RefreshToken.AddAsync(token);
            await context.SaveChangesAsync();

            token.UserId = Guid.NewGuid();

            // Act
            await repository.Update(token);

            // Assert
            var result = await context.RefreshToken.FirstAsync();
            Assert.Equal(token.Token, result.Token);
            Assert.Equal(token.UserId, result.UserId);
        }
    }
}
