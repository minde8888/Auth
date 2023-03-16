
namespace Auth.Domain.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException() : base("User not found please try again")
        { }
    }
}
