

using Auth.Domain.Entities.Auth;

namespace Auth.Domain.Interfaces
{
    public interface ITokenRepository
    {
        Task AddTokenAsync(RefreshToken token);
        Task<RefreshToken> GetToken(string token);
        Task Update(RefreshToken token);
    }
}
