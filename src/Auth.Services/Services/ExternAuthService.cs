using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Google.Apis.Auth;
using Auth.Services.WrapServices;
using Auth.Domain.Interfaces;
using FluentValidation;

namespace Auth.Services.Services
{
    public class ExternAuthService : IExternAuthService
    {
        private readonly IAuthApi _authApi;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<ExternalAuth> _externAuthValidator;
        private readonly IExternAuth _externAuth;

        public ExternAuthService(IAuthApi authApi,
            IUserRepository userRepository,
            ITokenService tokenService,
            IValidator<ExternalAuth> externAuthValidator,
            IExternAuth externAuth)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(authApi));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _externAuthValidator = externAuthValidator ?? throw new ArgumentNullException(nameof(externAuthValidator));
            _externAuth = externAuth ?? throw new ArgumentNullException(nameof(externAuth)); ;
        }

        public async Task<AuthResult> GoogleAuth(ExternalAuth googleAuth)
        {
            var validationResult = await _externAuthValidator.ValidateAsync(googleAuth);

            if (!validationResult.IsValid)
            {
                return new AuthResult
                {
                    Errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList(),
                    Success = false
                };
            }

            if (googleAuth.Provider != "google.com")
                throw new ExternalAuthException();

            var payload = await _externAuth.GoogleJsonValidaror(googleAuth.AccessToken);

            if (string.IsNullOrEmpty(payload.Email))
                throw new ExternalAuthException();

            var user = await _userRepository.GetUserByEmail(payload.Email);

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    Roles = "Basic",
                    Email = payload.Email,
                    UserName = payload.GivenName
                };

                var isCreated = _authApi.CreateBasicUser(user);

                if (isCreated.Result != null && !isCreated.Result.Succeeded)
                    throw new ExternalAuthException();

                await _authApi.AddRoleAsync(user, user.Roles);
            }

            return await _tokenService.GenerateJwtTokenAsync(user);
        }
    }
}
