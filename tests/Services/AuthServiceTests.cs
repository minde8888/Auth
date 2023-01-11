using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.MapperProfile;
using Auth.Services.Services;
using Auth.Services.Validators;
using Auth.Services.WrapServices;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthApi> _authApiMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly InlineValidator<Signup> _signupValidator;
        private readonly InlineValidator<Login> _loginValidator;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authApiMock = new Mock<IAuthApi>();
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            _signupValidator = new InlineValidator<Signup>();
            _loginValidator = new InlineValidator<Login>();

            _authService = new AuthService(_mapper, _userRepositoryMock.Object, _authApiMock.Object,  _signupValidator, _loginValidator);
        }
    }
}
