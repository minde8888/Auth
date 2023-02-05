namespace Auth.Domain.Entities
{
    public class LoginResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
        public UserResponse User { get; set; }
    }
}
