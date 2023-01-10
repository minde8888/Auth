using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.MapperProfile;
using Auth.Services.Services;
using Auth.Services.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            var store = new Mock<IUserStore<IdentityUser>>();
            //store.Setup(x => x. .FindByIdAsync("123", CancellationToken.None))
            //  .ReturnsAsync(new IdentityUser()
            //  {
            //      PhoneNumber = "12345678",
            //      Email = "123"
            //  });
            //var userManager = new UserManager<ApplicationUser>((IUserStore<ApplicationUser>)store.Object, null, null, null, null, null, null, null, null);
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MapperProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            //_authService = new AuthService(_mapper, _userRepositoryMock.Object, userManager, _tokenService);
        }
    }
}
