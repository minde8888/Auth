using Auth.Data.Configuration;
using Auth.Data.Context;
using Auth.Data.Repositories;
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Auth.Domain.Interfaces;
using Auth.Services;
using Auth.Services.Dtos.Auth;
using Auth.Services.MapperProfile;
using Auth.Services.Services;
using Auth.Services.Validators;
using Auth.Services.WrapServices;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class DependencyInjection
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(MapperProfile));

        services.AddTransient<AuthService>();
        services.AddTransient<ITokenService,TokenService>();

        services.AddTransient<IAuthApi, AuthApi>();
        services.AddTransient<ITokenApi , TokenApi>();

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ITokenRepository, TokenRepository>();

        services.AddTransient<UserManager<ApplicationUser>>();

        services.AddTransient<GoogleTokenValidator>();
        services.AddTransient<IValidator<ExternalAuth>, GoogleAuthValidator>();
        services.AddTransient<IValidator<Login>, LoginValidator>();
        services.AddTransient<IValidator<RequestToken>, RequestTokenValidator>();
        services.AddTransient<IValidator<Signup>, SignupValidator>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(o => o.SignIn.RequireConfirmedAccount = true)
         .AddRoles<ApplicationRole>()
         .AddRoleManager<RoleManager<ApplicationRole>>()
         .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));

        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidIssuer = configuration["JwtConfig:Issuer"],
            ValidAudience = configuration["JwtConfig:Audience"],
            ClockSkew = TimeSpan.Zero,
        };

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(jwt =>
        {
            jwt.SaveToken = true;
            jwt.TokenValidationParameters = tokenValidationParameters;
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"];
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });

        services.AddSingleton(tokenValidationParameters);

        var connectionString = Environment.GetEnvironmentVariable("DockerCommandsConnectionString");
        services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    }
}
