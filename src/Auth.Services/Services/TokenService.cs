using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services.Dtos.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Services.Services
{
    public class TokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public TokenService
            (ITokenRepository tokenRepository,
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<JwtConfig> jwtConfig)
        {

            _jwtConfig = jwtConfig.CurrentValue;
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public RefreshToken GetRefreshToken(SecurityToken token, string rand, ApplicationUser user)
        {
            RefreshToken refreshToken = new()
            {
                JwtId = token.Id,
                IsUsed = false,
                UserId = user.Id,
                AddedDate = token.ValidFrom,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Expires = token.ValidTo,
                IsRevoked = false,
                Token = rand
            };
            return refreshToken;
        }

        public async Task<AuthResult> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            roles.ToList().ForEach(role =>
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            });

            var a = _jwtConfig.Issuer;
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("guid", user.Id.ToString()),
                }.Union(roleClaims)),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var rand = RandomString(36);
            var refreshToken = GetRefreshToken(token, rand, user);

            await _tokenRepository.AddTokenAsync(refreshToken);

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResult> VerifyToken(RequestToken tokenRequest, ClaimsPrincipal principal, SecurityToken validatedToken)
        {
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (result == false)
                {
                    return null;
                }
            }

            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expDate = UnixTimeStampToDateTime(utcExpiryDate);

            if (expDate < DateTime.UtcNow)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "We cannot refresh this since the token has not expired" },
                    Success = false
                };
            }

            var storedRefreshToken = await _tokenRepository.GetToken(tokenRequest.RefreshToken);

            if (storedRefreshToken == null)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "refresh token doesn't exist" },
                    Success = false
                };
            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
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

            var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthResult()
                {
                    Errors = new List<string>() { "the token doesn't match the saved token" },
                    Success = false
                };
            }

            storedRefreshToken.IsUsed = true;

            await _tokenRepository.Update(storedRefreshToken);

            var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());
            return await GenerateJwtTokenAsync(dbUser);
        }

        private string RandomString(int length)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }
    }
}
