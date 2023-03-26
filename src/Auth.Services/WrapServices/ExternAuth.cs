using Auth.Domain.Exceptions;
using Google.Apis.Auth;

namespace Auth.Services.WrapServices
{
    public class ExternAuth : IExternAuth
    {
        public async Task<GoogleJsonWebSignature.Payload> GoogleJsonValidaror(string token)
        {
            return await GoogleJsonWebSignature.ValidateAsync(token) ?? throw new ExternalAuthException();
        }
    }
}
