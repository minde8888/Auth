using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Google.Apis.Auth;
using Auth.Services.WrapServices;
using Auth.Domain.Interfaces;
using FluentValidation;
using RestSharp;

namespace Auth.Services.Services
{
    public class ExternAuthService : IExternAuthService
    {
        private readonly IAuthApi _authApi;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<ExternalAuth> _externAuthValidator;

        public ExternAuthService(IAuthApi authApi,
            IUserRepository userRepository,
            ITokenService tokenService,
            IValidator<ExternalAuth> externAuthValidator)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(authApi));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _externAuthValidator = externAuthValidator ?? throw new ArgumentNullException(nameof(externAuthValidator));
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

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleAuth.AccessToken) ?? throw new ExternalAuthException();

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

                if (!isCreated.Result.Succeeded)
                    throw new ExternalAuthException();

                await _authApi.AddRoleAsync(user, user.Roles);
            }

            return await _tokenService.GenerateJwtTokenAsync(user);
        }

        public async Task<AuthResult> FacebookAuth(ExternalAuth facebookAuth)
        {
            var client = new RestClient("https://graph.facebook.com/v8.0");
            var request = new RestRequest($"me?access_token={facebookAuth.AccessToken}");
            var response = await client.GetAsync(request);

            if (!response.IsSuccessful)
                throw new ExternalAuthException();

            //var data = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content!);
            //var facebookId = new Guid(data!["id"]);
            //var name = data["name"];

            //var account = _userRepository.GetUser<User>(facebookId);
            //_context.BaseUser.FirstOrDefault(x => x.FacebookId == facebookId);

            //create new account if first time logging in
            //if (account == null)
            //{

            //    _userRepository.AddUserAsync<User>();
            //}

            //var token = GenerateJwtTokenAsync(account);

            return null;
        }
    }
}
