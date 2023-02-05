using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.MapperProfile;
using Auth.Services.WrapServices;
using AutoMapper;
using FluentValidation;
using Moq;

namespace tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthApi> _authApiMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        private readonly IMapper _mapper;
        private readonly Mock<IUserRepository> _userRepositoryMock;        

        private readonly InlineValidator<Signup> _signupValidator;
        private readonly InlineValidator<Login> _loginValidator;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {    
            _authApiMock = new Mock<IAuthApi>();
            _tokenServiceMock = new Mock<ITokenService>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile()); 
            });

            _mapper = mappingConfig.CreateMapper();

            _userRepositoryMock = new Mock<IUserRepository>();

            _signupValidator = new InlineValidator<Signup>();
            _loginValidator = new InlineValidator<Login>();

            _authService = new AuthService(
                _authApiMock.Object, 
                _mapper, 
                _userRepositoryMock.Object, 
                _tokenServiceMock.Object, 
                _signupValidator, 
                _loginValidator);

          }
    }
}
