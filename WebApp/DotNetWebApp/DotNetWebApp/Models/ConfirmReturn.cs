namespace DotNetWebApp.Models
{
	public class ConfirmReturn
	{
		public int Id { get; set; }
		public string Client_Id { get; set; } = "";
		public string Email { get; set; } = "";
		public string Platform { get; set; } = "";
		public bool IsReturned { get; set; }
		public bool IsReadyToReturn { get; set; }
		public IFormFile? File { get; set; }
	}
}
