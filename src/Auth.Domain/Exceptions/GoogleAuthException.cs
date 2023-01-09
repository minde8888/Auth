
namespace Auth.Domain.Exceptions
{
    public class GoogleAuthException : Exception
    {
        public GoogleAuthException() : base("Google login is not allowed") { }
    }
}
