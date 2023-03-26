using Auth.Data.Context;
using Auth.Data.Repositories;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace tests.Repository
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        public UserRepositoryTests()
        {
            // Setup the in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "test_database")
                .Options;

            // Initialize the context and repository
            _context = new AppDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        public void Dispose()
        {
            // Cleanup the in-memory database
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task AddUserAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                PasswordHash = "password"
            };

            // Act
            await _userRepository.AddUserAsync(user);

            // Assert
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            Assert.NotNull(result);
            Assert.Equal(user.PasswordHash, result.PasswordHash);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = new ApplicationUser
            {
                Email = email,
                PasswordHash = "password"
            };
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.PasswordHash, result.PasswordHash);
        }
    }
}
