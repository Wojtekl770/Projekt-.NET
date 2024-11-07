using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotNetWebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace DotNetWebApp.Data
{
	public class ApplicationDbContext : IdentityDbContext<CustomUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.HasDefaultSchema("Identity");
			builder.Entity<CustomUser>(e => e.ToTable(name: "User"));
			builder.Entity<IdentityUserRole<string>>(e => e.ToTable(name: "Role"));
			builder.Entity<IdentityUserClaim<string>>(e => e.ToTable(name: "UserClaims"));
			builder.Entity<IdentityUserLogin<string>>(e => e.ToTable(name: "UserLogins"));
			builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable(name: "RoleClaims"));
			builder.Entity<IdentityUserToken<string>>(e => e.ToTable(name: "UserTokens"));

		}

		public DbSet<CustomUser> Users { get; set; }
	}
}
