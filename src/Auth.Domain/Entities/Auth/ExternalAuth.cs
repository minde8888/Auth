

namespace Auth.Domain.Entities.Auth
{
    public class ExternalAuth
    {
        public ProviderType Provider { get; set; }
        public string AccessToken { get; set; }

        public IEnumerable<string> Validate()
        {
            if (Provider != ProviderType.Google && Provider != ProviderType.Facebook)
                yield return "Invalid provider type.";

            if (string.IsNullOrEmpty(AccessToken))
                yield return "Access token is required.";
        }
    }

    public enum ProviderType
    {
        Google,
        Facebook
    }

}
