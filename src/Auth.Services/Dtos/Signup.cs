

using System.ComponentModel.DataAnnotations;

namespace Auth.Services.Dtos
{
    public class Signup
    {
        public Guid UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Roles { get; set; }
    }
}
