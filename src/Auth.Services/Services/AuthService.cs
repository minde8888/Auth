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
            _loginValidator = loginValidator ?? throw new ArgumentException(nameof(loginValidator));
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

            user.UserId = newUser.Id;

            switch (user.Roles)
            {
                case "SuperAdmin":
                    var superAdmin = _mapper.Map<SuperAdmin>(user);
                    if (superAdmin == null)
                        throw new SuperAdminNotExistException();

                    await _userRepository.AddUserAsync(superAdmin);
                    break;

                default:
                    throw new RoleNotExistException();
            }

            return new SignupResponse()
            {
                Success = isCreated.Succeeded
            };
        }

        public async Task<LoginResult> GetUserAsync(Login login, string imageSrc)
        {
            var validationResult = await _loginValidator.ValidateAsync(login);

            if (!validationResult.IsValid)
            {
                return new LoginResult
                {
                    Errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList(),
                    Success = false
                };
            }

            var user = await _authApi.GetUserAsync(login.Email);

            if (user == null)
            {
                return new LoginResult
                {
                    Errors = new List<string> { "The email address is incorrect. Please retry." },
                    Success = false
                };
            }

            if (user.IsDeleted)
            {
                return new LoginResult
                {
                    Errors = new List<string> { "Signup account was deleted. Please contact support." },
                    Success = false
                };
            }

            var isPasswordValid = await _authApi.PasswordValidatorAsync(user, login.Password);

            if (!isPasswordValid)
            {
                return new LoginResult()
                {
                    Errors = new List<string>() {
                                "The password is incorrect. Please try again."//throw
                            },
                    Success = false
                };
            }

            var token = await _tokenService.GenerateJwtTokenAsync(user);
            var roles = await _authApi.RolesAsync(user);

            var superAdmin = await _userRepository.GetUser<SuperAdmin>(user.Id);

            if (superAdmin == null)
            {
                throw new UserNotFoundException();
            }

            if (roles.Contains("SuperAdmin"))
            {
                return new LoginResult
                {
                    Token = token.Token,
                    RefreshToken = token.RefreshToken,
                    Success = true,
                    User = _mapper.Map<UserResponse>(superAdmin)
                };
            }

            throw new RoleNotExistException();
        }
    }
}

