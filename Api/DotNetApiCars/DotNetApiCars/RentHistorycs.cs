using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetApiCars
{
	public enum RentState
	{
		In_Progess = 0,
		Active = 1,
		ReadyToReturn = 2,
		Returned = 3,
		Failure = 4
	}

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
		public bool IsReturned { get; set; }
		public bool IsReadyToReturn { get; set; }

		public RentState RentState { get; set; }
		public RentHistory() { }

	}
}
