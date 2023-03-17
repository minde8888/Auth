

namespace Auth.Domain.Entities.Auth
{
    public class ExternalAuth
    {
        public string Provider { get; set; }
        public string AccessToken { get; set; }
    }
}
