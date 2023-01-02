using Auth.Data.Context;

namespace Auth.Data.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync<T>(T t) 
        {
            await _context.AddAsync(t);
            await _context.SaveChangesAsync();
        }
    }
}
