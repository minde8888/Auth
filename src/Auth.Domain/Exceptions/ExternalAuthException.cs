
namespace Auth.Domain.Exceptions
{
    public class ExternalAuthException : Exception
    {
        public ExternalAuthException() : base("Login/signup is not allowed") { }
    }
}
