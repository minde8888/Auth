using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services.Dtos.Auth;
using Auth.Services.Validators;
using FluentValidation;
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
        private readonly GoogleTokenValidator _googleTokenValidator;
        private readonly IValidator<RequestToken> _requestTokenValidator;
        private readonly IValidator<GoogleAuth> _googleValidator;

        public TokenService
            (ITokenRepository tokenRepository,
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<JwtConfig> jwtConfig,
            GoogleTokenValidator googleTokenValidator,
            IValidator<RequestToken> requestTokenValidator,
            IValidator<GoogleAuth> googleValidator)
        {
            _jwtConfig = jwtConfig.CurrentValue;
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _googleTokenValidator = googleTokenValidator ?? throw new ArgumentNullException(nameof(googleTokenValidator));
            _requestTokenValidator = requestTokenValidator ?? throw new ArgumentNullException(nameof(requestTokenValidator));
            _googleValidator = googleValidator ?? throw new ArgumentNullException(nameof(googleValidator));
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
            var validationResult = await _requestTokenValidator.ValidateAsync(tokenRequest);
            if (validationResult.IsValid)
            {
                var storedRefreshToken = await _tokenRepository.GetToken(tokenRequest.RefreshToken);

                _googleTokenValidator.TokenValidatorAsync(principal, validatedToken, storedRefreshToken);

                storedRefreshToken.IsUsed = true;
                await _tokenRepository.Update(storedRefreshToken);

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId.ToString());
                return await GenerateJwtTokenAsync(dbUser);
            }

            var errorList = new List<string>();
            foreach (var error in validationResult.Errors)
            {
                errorList.Add(error.ErrorMessage);
            }
            return new AuthResult()
            {
                Errors = errorList,
                Success = false
            };
        }

        private string RandomString(int length)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public async Task<AuthResult> GetGoogleTokenAsync(GoogleAuth googleAuth)
        {
            var validationResult = _googleValidator.Validate(googleAuth);
            if (validationResult.IsValid)
            {
                var payload = _googleTokenValidator.VerifyGoogleToken(googleAuth).Result;
                if (payload == null)
                    throw new GoogleAuthException();

                var info = new UserLoginInfo(googleAuth.Provider, payload.Subject, googleAuth.Provider);
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user == null)
                    throw new GoogleAuthException();

                var result = await GenerateJwtTokenAsync(user);
                return result;
            }
            var errorList = new List<string>();
            foreach (var error in validationResult.Errors)
            {
                errorList.Add(error.ErrorMessage);
            }
            return new AuthResult()
            {
                Errors = errorList,
                Success = false
            };
        }
    }
}

