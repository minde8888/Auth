using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services.Dtos;
using Auth.Services.Services;
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
        private readonly TokenService _tokenService;

        private readonly IValidator<Signup> _signupValidator;
        private readonly IValidator<Login> _loginValidator;

        public AuthService(IAuthApi authApi,
            IMapper mapper,
            IUserRepository userRepository,
            TokenService tokenService,
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
                var errorList = new List<string>();
                foreach (var error in validationResult.Errors)
                {
                    errorList.Add(error.ErrorMessage);
                }
                return new SignupResponse()
                {
                    Errors = errorList,
                    Success = false
                };
            };

            var exist = _authApi.UserExisitAsync(user.PhoneNumber, user.Email);

            if (exist)
                throw new UserExistException();

            var newUser = new ApplicationUser()
            {
                Roles = user.Roles,
                Email = user.Email,
                UserName = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            var isCreated = await _authApi.CreateUserAsync(newUser, user.Password);

            var result = new SignupResponse();

            if (isCreated.Succeeded)
            {
                await _authApi.AddRoleAsync(newUser, user.Roles);

                user.UserId = newUser.Id;

                switch (user.Roles)
                {
                    case "SuperAdmin":
                        var superAdmin = _mapper.Map<SuperAdmin>(user);
                        if (superAdmin == null)
                            throw new SuperAdminNotExistException();

                        await _userRepository.AddUserAsync(superAdmin);

                        result.Success = isCreated.Succeeded;
                        break;

                    default:
                        throw new RoleNotExistException();
                }
            }
            else
            {
                return new SignupResponse()
                {
                    Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                    Success = false
                };
            }
            return new SignupResponse()
            {
                Success = isCreated.Succeeded
            };
        }

        public async Task<LoginResult> GetUserAsync(Login login, string imageSrc)
        {
            var validationResult = await _loginValidator.ValidateAsync(login);
            if (validationResult.IsValid)
            {
                ApplicationUser user = await _authApi.GetUserAsync(login.Email);

                if (user == null || user.IsDeleted)
                {
                    return new LoginResult()
                    {
                        Errors = new List<string>() {
                                "The email address is incorrect. Please retry."//throw
                            },
                        Success = false
                    };
                }
                if (user.IsDeleted)
                {
                    return new LoginResult()
                    {
                        Errors = new List<string>() {
                                "Signup account was deleted take contact with support"
                            },
                        Success = false
                    };
                }

                var isCorrect = await _authApi.PasswordValidatorAsync(user, login.Password);                    

                if (!isCorrect)
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

                foreach (var role in roles)
                {
                    switch (role)
                    {
                        case "SuperAdmin":
                            var superAdmin = await _userRepository.GetUser<SuperAdmin>(user.Id);
                            if (superAdmin == null)
                                throw new UserNotFoundException();

                            return new LoginResult()
                            {
                                Token = token.Token,
                                RefreshToken = token.RefreshToken,
                                Success = true,
                                User = _mapper.Map<User>(superAdmin)
                            };
                        default:
                            throw new RoleNotExistException();
                    };
                }
            }

            var errorList = new List<string>();
            foreach (var error in validationResult.Errors)
            {
                errorList.Add(error.ErrorMessage);
            }
            return new LoginResult()
            {
                Errors = errorList,
                Success = false
            };
        }
    }
}

