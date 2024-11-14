using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DotNetApiCars
{
	public class Offer
	{
		public int Id { get; set; }
		public int PriceDay { get; set; }
		public int PriceInsurance { get; set; }
		public DateTime ExpirationDate { get; set; }
		public bool IsSuccess { get; set; }
	}

	public class OfferDB
	{
		public int Id { get; set; }
		public int PriceDay { get; set; }
		public int PriceInsurance { get; set; }
		public DateTime ExpirationDate { get; set; }
		public DateTime WhenOfferWasMade { get; set; }
		public int CarId { get; set; }
		public Car Car { get; set; }
	}


}
