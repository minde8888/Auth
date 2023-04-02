using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities
{
    public class Signup
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "User Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "User Surname is required")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "User PhoneNumber is required")]
        [RegularExpression(@"^(\\+)?\d{8}$", ErrorMessage = "The PhoneNumber field must be an 8-digit phone number")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Roles { get; set; }
    }
}
