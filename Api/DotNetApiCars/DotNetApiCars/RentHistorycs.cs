namespace DotNetApiCars
{
	public class RentHistory
	{
		public int Id { get; set; }
		public int Client_Id { get; set; }
		public Car Car { get; set; }
		public string Platform { get; set; }
		public DateTime RentDate { get; set; }
		public RentHistory() { }

	}
}
