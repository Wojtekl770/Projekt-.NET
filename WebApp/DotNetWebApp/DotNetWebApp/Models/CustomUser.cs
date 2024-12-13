using Microsoft.AspNetCore.Identity;

namespace DotNetWebApp.Models
{
	public class CustomUser : IdentityUser
	{
		public int YearOfGettingDriversLicence { get; set; }
		public int YearOfBirth { get; set; }
		public string? Localization { get; set; }
		public string? Name { get; set; }
		public string? Surname { get; set; }
		public bool Worker { get; set; }
	}
}
