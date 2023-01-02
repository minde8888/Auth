

namespace Auth.Domain.Exceptions
{
    public class RoleNotExistException : Exception
    {
        public RoleNotExistException() : base("Role doesn't exist !!!")
        { }
    }
}
