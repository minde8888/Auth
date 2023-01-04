using Auth.Data.Context;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data.Repositories
{
    public class UserRepository: IUserRepository
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

        public async Task<T> GetUser<T>(Guid id) where T : BaseEntity
        {
            return await _context.Set<T>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
    }
}
