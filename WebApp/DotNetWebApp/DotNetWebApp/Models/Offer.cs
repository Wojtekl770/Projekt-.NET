using System.Data.SqlTypes;

namespace DotNetWebApp.Models
{
	public class Offer
	{
		public int Id { get; set; }
		public int PriceDay { get; set; }
		public int PriceInsurance { get; set; }
		public DateTime ExpirationDate { get; set; }
		public bool IsSuccess { get; set; }
	}

	public class OfferCarModel
	{
		public int Id { get; set; }
		public int PriceDay { get; set; }
		public int PriceInsurance { get; set; }
		public DateTime ExpirationDate { get; set; }
		public Car Car { get; set; }
		public int CarId { get; set; }
	}

	public class OfferCarID
	{
		public int Id { get; set; }
		public int CarId { get; set; }
	}

}
