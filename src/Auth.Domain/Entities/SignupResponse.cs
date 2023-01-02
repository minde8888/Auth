

namespace Auth.Domain.Entities
{
    public class SignupResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
