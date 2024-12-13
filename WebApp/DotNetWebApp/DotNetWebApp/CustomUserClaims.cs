using DotNetWebApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DotNetWebApp
{
	public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<CustomUser, IdentityRole>
	{
		public CustomUserClaimsPrincipalFactory(
			UserManager<CustomUser> userManager,
			RoleManager<IdentityRole> roleManager,
			Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor)
			: base(userManager, roleManager, optionsAccessor)
		{
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(CustomUser user)
		{
			var identity = await base.GenerateClaimsAsync(user);
			identity.AddClaim(new Claim("YearOfBirth", user.YearOfBirth.ToString()));
			identity.AddClaim(new Claim("YearOfGettingDriversLicence", user.YearOfGettingDriversLicence.ToString()));
			identity.AddClaim(new Claim("Name", user.Name == null ? "" : user.Name.ToString()));
			identity.AddClaim(new Claim("Surname", user.Surname == null ? "" : user.Surname.ToString()));
            identity.AddClaim(new Claim("Worker", user.Worker.ToString()));
            return identity;
		}
	}
}
