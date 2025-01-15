using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace DotNetWebApp.ObceApi
{

	public class ReturnRequestTheir
	{
		public string EmployeeEmail { get; set; }
		public string ReturnDescription { get; set; }
		public string Base64EncodedCarImage { get; set; }
	}

	public class Car2
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("modelName")]
		public string ModelName { get; set; }

		[JsonPropertyName("brandName")]
		public string BrandName { get; set; }

		[JsonPropertyName("productionYear")]
		public int ProductionYear { get; set; }
	}

	public class Offer
	{
		public int Id { get; set; }

		public Guid OfferGuid { get; set; }

		public bool IsInsurance { get; set; }

		public int CarId { get; set; }

		public decimal Price { get; set; }

		public DateTime ExpirationDate { get; set; }

		public string UserEmail { get; set; } = "";
	}

	public class RentalInfo
	{
		public int Id { get; set; }
		public string OfferGuid { get; set; } = "";
		public int RentalStatus { get; set; }
		public DateTime RentDate { get; set; }
		public int CarID { get; set; }
		public string UserEmail { get; set; } = "";
		public int SourceAPI { get; set; }
		public double PricePerDay { get; set; }
		public bool IsInsurance { get; set; }
		public Car Car { get; set; }
		public string? Return { get; set; }
	}

	public class Car
	{
		public int Id { get; set; }

		public int ProductionYear { get; set; }

		public int HorsePower { get; set; }

		public string FuelType { get; set; }

		public string Drive { get; set; }

		public string Transmission { get; set; }

		public int DoorsNumber { get; set; }

		public string Colour { get; set; }

		[Range(0, 1)]
		public int Availability { get; set; }

		[DataType(DataType.Currency)]
		public decimal PricePerDay { get; set; }

		[DataType(DataType.Currency)]
		public decimal InsurancePricePerDay { get; set; }

		public int ModelID { get; set; }

		public Location Location { get; set; }

		public Model Model { get; set; }
	}

	public class Location
	{
		public float Latitude { get; set; }

		public float Longitude { get; set; }
	}

	public class Model
	{
		[Required]
		[StringLength(250, MinimumLength = 1)]
		public string Name { get; set; }

		public int Id { get; set; }

		public int BrandID { get; set; }

		public Brand Brand { get; set; }
	}

	public class Brand
	{
		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Name { get; set; }

		public int Id { get; set; }
	}
}
