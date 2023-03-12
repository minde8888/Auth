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
        private readonly TokenValidationParameters _tokenValidationParams;

        public AuthController(
            ITokenService tokenService,
            TokenValidationParameters tokenValidationParams,
            AuthService authService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
        public async Task<IActionResult> Login(Login login)
        {
            var imageSrc = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.GetUserAsync(login, imageSrc);
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
            var response = await _tokenService.ValidateGoogleTokenAsync(googleAuth);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] ExternalAuth facebookAuth)
        {
            var response = await _tokenService.ValidateFacebookTokenAsync(facebookAuth);

            return Ok(response);
        }
    }
}