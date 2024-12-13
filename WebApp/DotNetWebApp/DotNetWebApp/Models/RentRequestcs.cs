namespace DotNetWebApp.Models
{
	public class RentRequest : RentsRequest
	{
		public int Rent_Id { get; set; }
		public RentRequest() : base() { }
	}

	public class RentsRequest
	{
		public string Client_Id { get; set; } = "";
		public string Platform { get; set; } = "";
		public string Email { get; set; } = "";
		public RentsRequest() { }
	}
}
