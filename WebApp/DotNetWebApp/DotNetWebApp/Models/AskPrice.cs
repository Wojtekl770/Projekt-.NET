namespace DotNetWebApp.Models
{
	public class AskPrice
	{
		public int Car_Id { get; set; }
		public int DriversLicenceDuration { get; set; }
		public int Age { get; set; }
		public DateTime Start { get; set; }
		public DateTime Return { get; set; }
		public string ExtraInfo { get; set; }

	}
}
