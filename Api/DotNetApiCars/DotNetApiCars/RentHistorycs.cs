using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetApiCars
{
	public class RentHistory
	{
		public int Id { get; set; }
		public string Client_Id { get; set; } = "";
		public string Name { get; set; }
		public string Surname { get; set; }
		public string Email { get; set; }
		public string Platform { get; set; }
		public DateTime RentDate { get; set; }
		public int OfferId { get; set; }
		public OfferDB Offer {get; set;}
		public RentHistory() { }

	}
}
