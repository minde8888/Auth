

namespace Auth.Domain.Exceptions
{
    public class UserExistException : Exception
    {
        public UserExistException() : base("Email or phone number is already exist !!!")
        { }
    }
}
