using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Domain.Entities.Auth
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public string JwtId { get; set; }
        public bool IsUsed { get; set; } 
        public bool IsRevoked { get; set; } 
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; } 

        [ForeignKey(nameof(UserId))]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
