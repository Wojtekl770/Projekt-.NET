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
		public CarPlatform Car { get; set; }
		public int CarId { get; set; }
		public string Platform { get; set; }
		public OfferCarModel() { }
		public OfferCarModel(OfferDB o, string platform = "")
		{
			this.Id = o.Id;
			this.PriceDay = o.PriceDay;
			this.PriceInsurance = o.PriceInsurance;
			this.ExpirationDate = o.ExpirationDate;
			this.CarId = o.CarId;
			this.Platform = platform;
		}
	}

	public class OfferCarID
	{
		public int Id { get; set; }
		public int CarId { get; set; }
		public string Platform { get; set; }
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
