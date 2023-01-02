using Auth.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Domain.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Roles { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageName { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
