namespace DotNetWebApp.Models
{
	public class OfferChoice
	{
		public string Client_Id { get; set; } = "";
		public int Offer_Id { get; set; }
		public string Platform { get; set; } = "";

		public string Name { get; set; } = "";
		public string Surname { get; set; } = "";
		public string Email { get; set; } = "";
	}
}
