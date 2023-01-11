using Auth.Domain.Entities.Auth;

namespace Auth.Services.WrapServices
{
    public interface ITokenApi
    {
        Task<IList<string>> RolesAsync(ApplicationUser user);
        Task<ApplicationUser> FindUserIdAsync(string id);
        Task<ApplicationUser> FindUserLoginAsync(string loginProvider, string providerKey);
    }
}
