
using Auth.Data.Context;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data.Repositories
{
    public class TokenRepository: ITokenRepository
    {
        private readonly AppDbContext _context;
        public TokenRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddTokenAsync(RefreshToken token)
        {
            _context.RefreshToken.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetToken(string token)
        {
            return await _context.RefreshToken.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task Update(RefreshToken token)
        {
            _context.RefreshToken.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
