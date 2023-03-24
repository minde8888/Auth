using Auth.Domain.Exceptions;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
