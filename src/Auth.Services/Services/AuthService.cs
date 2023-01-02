using Auth.Data.Configuration;
using Auth.Data.Repositories;
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Exceptions;
using Auth.Services.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;

        public AuthService(IMapper mapper, UserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<SignupResponse> CreateUserAsync(User user)
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
    }
}
