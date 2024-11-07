using Microsoft.AspNetCore.Identity;

namespace DotNetWebApp.Models
{
	public class CustomUser : IdentityUser
	{
		//[ProtectedPersonalData]
		public int YearOfGettingDriversLicence { get; set; }

		//[ProtectedPersonalData]
		public int YearOfBirth { get; set; }
		//[ProtectedPersonalData]
		public string? Localization { get; set; }
		public string? Name { get; set; }
		public string? Surname { get; set; }
	}
}
