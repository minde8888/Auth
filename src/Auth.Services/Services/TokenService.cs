using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services.Dtos.Auth;
using Auth.Services.WrapServices;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;

        private readonly JwtConfig _jwtConfig;

        private readonly IValidator<RequestToken> _requestTokenValidator;

        private readonly ITokenApi _tokenApi;

        public TokenService
            (ITokenRepository tokenRepository,
            IOptionsMonitor<JwtConfig> jwtConfig,
            IValidator<RequestToken> requestTokenValidator,
            ITokenApi tokenApi)
        {
            _jwtConfig = jwtConfig.CurrentValue;
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));

            _requestTokenValidator = requestTokenValidator ?? throw new ArgumentNullException(nameof(requestTokenValidator));

            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
          
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
            var roles = await _tokenApi.RolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("guid", user.Id.ToString())
            }.Union(roleClaims));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = signingCredentials
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = await GenerateRefreshTokenAsync(token, user);
            await _tokenRepository.AddTokenAsync(refreshToken);

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(SecurityToken token, ApplicationUser user)
        {
            var rand = RandomString(36);
            return await Task.FromResult(GetRefreshToken(token, rand, user));
        }

        private string RandomString(int length)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public async Task<AuthResult> VerifyToken(RequestToken tokenRequest, ClaimsPrincipal principal, SecurityToken validatedToken)
        {
            var validationResult = await _requestTokenValidator.ValidateAsync(tokenRequest);
            if (!validationResult.IsValid)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList()
                };
            }

            var storedRefreshToken = await _tokenRepository.GetToken(tokenRequest.RefreshToken);
            if (storedRefreshToken == null || storedRefreshToken.IsUsed)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid refresh token." }
                };
            }

            storedRefreshToken.IsUsed = true;
            await _tokenRepository.Update(storedRefreshToken);

            var user = await _tokenApi.FindUserIdAsync(storedRefreshToken.UserId.ToString());

            return await GenerateJwtTokenAsync(user);
        }
    }
}


