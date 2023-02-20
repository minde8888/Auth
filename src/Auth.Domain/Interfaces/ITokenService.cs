using Auth.Domain.Entities.Auth;
using Auth.Services.Dtos.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Auth.Domain.Interfaces
{
    public interface ITokenService
    {
        public RefreshToken GetRefreshToken(SecurityToken token, string rand, ApplicationUser user);
        public Task<AuthResult> GenerateJwtTokenAsync(ApplicationUser user);
        public Task<AuthResult> VerifyToken(RequestToken tokenRequest, ClaimsPrincipal principal, SecurityToken validatedToken);
        public Task<AuthResult> ValidateGoogleTokenAsync(ExternalAuth googleAuth);
        public Task<AuthResult> ValidateFacebookTokenAsync(ExternalAuth facebookAuth);
    }
}
