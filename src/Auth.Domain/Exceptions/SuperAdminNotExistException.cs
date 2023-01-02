
namespace Auth.Domain.Exceptions
{
    public class SuperAdminNotExistException : Exception
    {
        public SuperAdminNotExistException() : base("Request SuperAdmin doesn't exist.")
        { }
    }
}
