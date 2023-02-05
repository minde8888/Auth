
namespace Auth.Domain.Exceptions
{
    public class SuperAdminNotExistException : Exception
    {
        public SuperAdminNotExistException() : base("Request User doesn't exist.")
        { }
    }
}
