using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.Dtos.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AuthService _authService;
        private readonly IExternAuthService _externAuthService;
        private readonly TokenValidationParameters _tokenValidationParams;

        public AuthController(
            ITokenService tokenService,
            AuthService authService,
            IExternAuthService externAuthService,
            TokenValidationParameters tokenValidationParams)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _externAuthService = externAuthService ?? throw new ArgumentNullException(nameof(externAuthService));
            _tokenValidationParams = tokenValidationParams ?? throw new ArgumentNullException(nameof(tokenValidationParams));       
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] Signup user)
        {
            var result = await _authService.CreateUserAsync(user);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
         {
            var result = await _authService.AuthAsync(login);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RequestToken tokenRequest)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();

            _tokenValidationParams.ValidateLifetime = false;
            var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParams, out var validatedToken);
            _tokenValidationParams.ValidateLifetime = true;

            var response = await _tokenService.VerifyToken(tokenRequest, principal, validatedToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuth googleAuth)
        {
            var response = await _externAuthService.GoogleAuth(googleAuth);
            return Ok(response);
        }
    }
}