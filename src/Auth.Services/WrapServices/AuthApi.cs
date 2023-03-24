using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace Auth.Services.WrapServices
{
    public class AuthApi : IAuthApi
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthApi(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        public bool UserExisitAsync(string phoneNumber, string email)
        {
            return _userManager.Users.Any(u =>
               u.PhoneNumber == phoneNumber ||
               u.Email == email);
        }
        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> CreateBasicUser(ApplicationUser user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task AddRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<ApplicationUser> AuthUserAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> PasswordValidatorAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IList<string>> RolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
