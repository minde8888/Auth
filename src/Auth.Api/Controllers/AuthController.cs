using Auth.Domain.Entities;
using Auth.Services;
using Auth.Services.Dtos.Auth;
using Auth.Services.Services;
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
        private readonly TokenService _tokenService;
        private readonly AuthService _authService;
        private readonly TokenValidationParameters _tokenValidationParams;

        public AuthController(
            TokenService tokenService,
            TokenValidationParameters tokenValidationParams,
            AuthService authService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tokenValidationParams = tokenValidationParams ?? throw new ArgumentNullException(nameof(tokenValidationParams)); 
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Signup")]
        public async Task<IActionResult> Signup([FromBody] Signup user)
        {
            var result = await _authService.CreateUserAsync(user);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var imageSrc = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.GetUserAsync(login, imageSrc);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RequestToken tokenRequest)
        {
            JwtSecurityTokenHandler jwtTokenHandler = new();

            _tokenValidationParams.ValidateLifetime = false;
            var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParams, out var validatedToken);
            _tokenValidationParams.ValidateLifetime = true;
            var response = await _tokenService.VerifyToken(tokenRequest, principal, validatedToken);

            return Ok(response);
        }
    }
}