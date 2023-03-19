using Auth.Domain.Entities.Auth;

namespace Auth.Domain.Interfaces
{
    public interface IExternAuthService
    {
        Task<AuthResult> GoogleAuth(ExternalAuth googleAuth);
    }
}
