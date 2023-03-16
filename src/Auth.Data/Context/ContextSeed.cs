using Auth.Data.Configuration;
using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace Auth.Data.Context
{
    public static class ContextSeed
    {
        public static async Task SeedEssentialsAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            await roleManager.CreateAsync(new ApplicationRole(Authorization.Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(Authorization.Roles.Admin.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(Authorization.Roles.Moderator.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(Authorization.Roles.Basic.ToString()));

            var defaultUser = new ApplicationUser
            {
                UserName = Authorization.default_username,
                Email = Authorization.default_email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                await userManager.CreateAsync(defaultUser, Authorization.default_password);
                await userManager.AddToRoleAsync(defaultUser, Authorization.default_role.ToString());
            }
        }
    }
}
