using Auth.Data.Repositories;
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using Auth.Services.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Auth.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public AuthService(IMapper mapper,
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            TokenService tokenService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<SignupResponse> CreateUserAsync(Signup user)
        {
            var exist = _userManager.Users.Any(u =>
                u.PhoneNumber == user.PhoneNumber ||
                u.Email == user.Email);

            if (exist)
                throw new UserExistException();

            var newUser = new ApplicationUser()
            {
                Roles = user.Roles,
                Email = user.Email,
                UserName = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            var result = new SignupResponse();

            if (isCreated.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, user.Roles);

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
            ApplicationUser user = await _userManager.FindByEmailAsync(login.Email);

            if (user == null)
            {
                return new LoginResult()
                {
                    Errors = new List<string>() {
                                "The email address is incorrect. Please retry."
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

            var isCorrect = await _userManager.CheckPasswordAsync(user, login.Password);

            if (!isCorrect)
            {
                return new LoginResult()
                {
                    Errors = new List<string>() {
                                "The password is incorrect. Please try again."
                            },
                    Success = false
                };
            }

            var token = await _tokenService.GenerateJwtTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

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
                            User = superAdmin,
                        };
                    default:
                        throw new RoleNotExistException();

                };
            }

            return new LoginResult()//validation error
            {
                Errors = new List<string>(),
            };
        }
    }
}

