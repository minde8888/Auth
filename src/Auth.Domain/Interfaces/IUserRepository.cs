
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;

namespace Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync<T>(T t);
        Task<T> GetUser<T>(Guid id) where T : BaseUser;
        Task<ApplicationUser> GetUserByEmail(string email);
    }
}
