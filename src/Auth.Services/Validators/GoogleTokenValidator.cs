using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Services.Validators
{
    public class GoogleTokenValidator
    {
        private readonly GoolgeConfig _goolgeConfig;

        public GoogleTokenValidator(IOptionsMonitor<GoolgeConfig> goolgeConfig)
        {
            _goolgeConfig = goolgeConfig.CurrentValue;
        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyExternalToken(ExternalAuth googleAuth)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _goolgeConfig.ClientId }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleAuth.IdToken, settings);

            return payload ?? throw new ExternalAuthException();
        }

        public AuthResult TokenValidatorAsync(ClaimsPrincipal principal, SecurityToken validatedToken, RefreshToken storedRefreshToken)
        {
            if (!IsValidJwt(validatedToken))
            {
                return null;
            }

            if (IsExpired(principal))
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "We cannot refresh this since the token has not expired" },
                    Success = false
                };
            }

            if (storedRefreshToken == null)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "refresh token doesn't exist" },
                    Success = false
                };
            }

            if (IsRefreshTokenExpired(storedRefreshToken))
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "token has expired, user needs to reloading" },
                    Success = false
                };
            }

            if (storedRefreshToken.IsUsed)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "token has been used" },
                    Success = false
                };
            }

            if (storedRefreshToken.IsRevoked)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "token has been revoked" },
                    Success = false
                };
            }

            if (!IsRefreshTokenValid(principal, storedRefreshToken))
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "the token doesn't match the saved token" },
                    Success = false
                };
            }

            return new AuthResult()
            {
                Success = true
            };
        }

        private bool IsValidJwt(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsExpired(ClaimsPrincipal principal)
        {
            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expDate = UnixTimeStampToDateTime(utcExpiryDate);
            return expDate < DateTime.UtcNow;
        }

        private bool IsRefreshTokenExpired(RefreshToken storedRefreshToken)
        {
            return DateTime.UtcNow > storedRefreshToken.ExpiryDate;
        }

        private bool IsRefreshTokenValid(ClaimsPrincipal principal, RefreshToken storedRefreshToken)
        {
            var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            return storedRefreshToken.JwtId == jti;
        }


        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        }
    }
}
