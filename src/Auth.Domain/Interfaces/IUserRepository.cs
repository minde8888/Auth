
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync<T>(T t);
        Task<T> GetUser<T>(Guid id) where T : BaseUser;
    }
}
