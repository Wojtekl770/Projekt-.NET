using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetWebApp.Models
{
	public class Car
	{
		public int Id { get; set; }
		public string LicensePlate { get; set; }
		public string CarBrand { get; set; }
		public string CarModel { get; set; }
		public bool IsRented { get; set; }
		public string Localization { get; set; }

		public Car()
		{
			int Id = -1;
			string LicensePlate = "v";
			string CarBrand = "u";
			string CarModel = "c";
			bool IsRented = false;
			string Localization = "Warszawa";

			this.Id = Id;
			this.LicensePlate = LicensePlate;
			this.CarBrand = CarBrand;
			this.CarModel = CarModel;
			this.IsRented = IsRented;
			this.Localization = Localization;
		}

		public static bool operator ==(Car? f, Car? s)
		{
			if (f == null && s == null)
				return true;

			if (f == null || s == null)
				return false;

			return f.Id == s.Id && f.LicensePlate == s.LicensePlate && f.CarModel == s.CarModel;
		}
		public static bool operator !=(Car? f, Car? s)
		{
			return !(f == s);
		}


	}
}
