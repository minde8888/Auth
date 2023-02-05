using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services.Dtos.Auth;
using Auth.Services.Validators;
using Auth.Services.WrapServices;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Auth.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;

        private readonly JwtConfig _jwtConfig;

        private readonly GoogleTokenValidator _googleTokenValidator;
        private readonly IValidator<ExternalAuth> _googleAuthValidator;
        private readonly IValidator<RequestToken> _requestTokenValidator;

        private readonly ITokenApi _tokenApi;

        public TokenService
            (ITokenRepository tokenRepository,
            IUserRepository userRepository,
            IOptionsMonitor<JwtConfig> jwtConfig,
            GoogleTokenValidator googleTokenValidator,
            IValidator<RequestToken> requestTokenValidator,
            IValidator<ExternalAuth> googleAuthValidator,
            ITokenApi tokenApi)
        {
            _jwtConfig = jwtConfig.CurrentValue;
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

            _googleTokenValidator = googleTokenValidator ?? throw new ArgumentNullException(nameof(googleTokenValidator));
            _requestTokenValidator = requestTokenValidator ?? throw new ArgumentNullException(nameof(requestTokenValidator));
            _googleAuthValidator = googleAuthValidator ?? throw new ArgumentNullException(nameof(googleAuthValidator));

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

            var googleTokenValidationResult = _googleTokenValidator.TokenValidatorAsync(principal, validatedToken, storedRefreshToken);
            if (!googleTokenValidationResult.Success)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid Google token." }
                };
            }

            storedRefreshToken.IsUsed = true;
            await _tokenRepository.Update(storedRefreshToken);

            var user = await _tokenApi.FindUserIdAsync(storedRefreshToken.UserId.ToString());

            return await GenerateJwtTokenAsync(user);
        }

        public async Task<AuthResult> ValidateGoogleTokenAsync(ExternalAuth googleAuth)
        {
            var validationResult = _googleAuthValidator.Validate(googleAuth);
            if (!validationResult.IsValid)
            {
                return new AuthResult
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    Success = false
                };
            }

            var payload = _googleTokenValidator.VerifyExternalToken(googleAuth).Result;
            if (payload == null)
                throw new ExternalAuthException();

            var info = new UserLoginInfo(googleAuth.Provider, payload.Subject, googleAuth.Provider);

            var account = await _tokenApi.FindUserLoginAsync(info.LoginProvider, info.ProviderKey);
            if (account == null)
                throw new ExternalAuthException();

            return await GenerateJwtTokenAsync(account);
        }

        public async Task<AuthResult> ValidateFacebookTokenAsync(ExternalAuth facebookAuth)
        {
            var client = new RestClient("https://graph.facebook.com/v8.0");
            var request = new RestRequest($"me?access_token={facebookAuth.AccessToken}");
            var response = await client.GetAsync(request);

            if (!response.IsSuccessful)
                throw new ExternalAuthException();

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content!);
            var facebookId = new Guid(data!["id"]);
            var name = data["name"];

            var account = _userRepository.GetUser<User>(facebookId);
                //_context.BaseUser.FirstOrDefault(x => x.FacebookId == facebookId);

            // create new account if first time logging in
            //if (account == null)
            //{

            //    _userRepository.AddUserAsync<User>();
            //}

            //var token = GenerateJwtTokenAsync(account);

            return null;
        }
    }
}


