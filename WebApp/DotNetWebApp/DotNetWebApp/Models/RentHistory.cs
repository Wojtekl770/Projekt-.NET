namespace DotNetWebApp.Models
{
	public class RentHistory
	{
		public int Id { get; set; }
		public string Client_Id { get; set; } = "";
		public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Email { get; set; } = "";
		public string Platform { get; set; } = "";
        public DateTime RentDate { get; set; }
		public int OfferId { get; set; }
		public OfferDB Offer { get; set; }
        public bool IsReturned { get; set; }
        public RentHistory() { }

	}

    public class RentHistoryModel
    {
        public int Id { get; set; }
        public string Client_Id { get; set; } = "";
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Platform { get; set; }
        public DateTime RentDate { get; set; }
        public int OfferId { get; set; }
        public OfferCarModel Offer { get; set; }
        public bool IsReturned { get; set; }
        public RentHistoryModel() { }
        public RentHistoryModel(RentHistory rent) 
        {
            this.Id = rent.Id;
            this.Client_Id = rent.Client_Id;
            this.Name = rent.Name;
            this.Surname = rent.Surname;
            this.Email = rent.Email;
            this.Platform = rent.Platform;
            this.RentDate = rent.RentDate;
            this.OfferId = rent.OfferId;
            this.IsReturned = rent.IsReturned;
        }
    }
}
