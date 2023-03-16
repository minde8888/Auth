using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services.WrapServices;
using AutoMapper;
using FluentValidation;

namespace Auth.Services
{
    public class AuthService
    {
        private readonly IAuthApi _authApi;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        private readonly IValidator<Signup> _signupValidator;
        private readonly IValidator<Login> _loginValidator;

        public AuthService(IAuthApi authApi,
            IMapper mapper,
            IUserRepository userRepository,
            ITokenService tokenService,
            IValidator<Signup> signupValidator,
            IValidator<Login> loginValidator)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

            _signupValidator = signupValidator ?? throw new ArgumentNullException(nameof(signupValidator));
            _loginValidator = loginValidator ?? throw new ArgumentException(null, nameof(loginValidator));
        }

        public async Task<SignupResponse> CreateUserAsync(Signup user)
        {
            var validationResult = await _signupValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return new SignupResponse()
                {
                    Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    Success = false
                };
            }

            if (_authApi.UserExisitAsync(user.PhoneNumber, user.Email))
                throw new UserExistException();

            var newUser = new ApplicationUser()
            {
                Roles = user.Roles,
                Email = user.Email,
                UserName = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            var isCreated = await _authApi.CreateUserAsync(newUser, user.Password);

            if (!isCreated.Succeeded)
            {
                return new SignupResponse()
                {
                    Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                    Success = false
                };
            }

            await _authApi.AddRoleAsync(newUser, user.Roles);

            return new SignupResponse()
            {
                Success = isCreated.Succeeded
            };
        }

        public async Task<AuthResult> GetUserAsync(Login login)
        {
            var validationResult = await _loginValidator.ValidateAsync(login);

            if (!validationResult.IsValid)
            {
                return new AuthResult
                {
                    Errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList(),
                    Success = false
                };
            }

            var user = await _authApi.GetUserAsync(login.Email);

            if (user == null)
                throw new UserNotFoundException();

            if (user.IsDeleted)
                throw new UserNotFoundException();

            var isPasswordValid = await _authApi.PasswordValidatorAsync(user, login.Password);

            if (!isPasswordValid)
                throw new UserNotFoundException();

            return await _tokenService.GenerateJwtTokenAsync(user);
        }
    }
}

