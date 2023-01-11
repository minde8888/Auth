using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace Auth.Services.WrapServices
{
    public interface IAuthApi
    {
        bool UserExisitAsync(string phoneNumber, string email);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task AddRoleAsync(ApplicationUser user, string role);
        Task<ApplicationUser> GetUserAsync(string email);
        Task<bool> PasswordValidatorAsync(ApplicationUser user, string password);
        Task<IList<string>> RolesAsync(ApplicationUser user);
    }
}
