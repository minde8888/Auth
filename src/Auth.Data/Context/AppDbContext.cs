using Auth.Data.Configuration;
using Auth.Domain.Entities;
using Auth.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }

        public DbSet<IdentityUserClaim<Guid>> IdentityUserClaims { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<User> SuperAdmin { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("Identity");

            builder.Entity<User>().HasQueryFilter(p => p.IsDeleted == false);
        }
    }
}
