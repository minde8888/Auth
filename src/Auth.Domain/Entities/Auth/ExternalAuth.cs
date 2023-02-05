
namespace Auth.Domain.Entities.Auth
{
    public class ExternalAuth
    {
        public string AccessToken { get; set; }
        public string Provider { get; set; }
        public string IdToken { get; set; }
    }
}
