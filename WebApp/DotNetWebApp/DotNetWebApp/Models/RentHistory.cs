using DotNetWebApp.ObceApi;

namespace DotNetWebApp.Models
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
		public string Name { get; set; } = "";
		public string Surname { get; set; } = "";
		public string Email { get; set; } = "";
		public string Platform { get; set; } = "";
		public DateTime RentDate { get; set; }
		public int OfferId { get; set; }
		public OfferDB Offer { get; set; }
		public bool IsReturned { get; set; }
		public bool IsReadyToReturn { get; set; }
		public RentState RentState { get; set; }
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
		public bool IsReadyToReturn { get; set; }
		public RentState RentState { get; set; }
		public RentHistoryModel() { }
		public RentHistoryModel(RentHistory rent, string platform = "")
		{
			this.Id = rent.Id;
			this.Client_Id = rent.Client_Id;
			this.Name = rent.Name;
			this.Surname = rent.Surname;
			this.Email = rent.Email;
			this.Platform = platform;
			this.RentDate = rent.RentDate;
			this.OfferId = rent.OfferId;
			this.IsReturned = rent.IsReturned;
			this.IsReadyToReturn = rent.IsReadyToReturn;
			this.RentState = rent.RentState;
		}
		public RentHistoryModel(ObceApi.RentalInfo rent, OfferChoice offerChoice, string platform = "")
		{
			this.Id = rent.Id;
			this.Client_Id = offerChoice.Client_Id;
			this.Name = offerChoice.Name;
			this.Surname = offerChoice.Surname;
			this.Email = offerChoice.Email;
			this.Platform = platform;
			this.RentDate = rent.RentDate;
			this.OfferId = offerChoice.Offer_Id;
			this.IsReturned = false;
			this.IsReadyToReturn = false;

			this.RentState = ConverterEnumState.Convert((RentalStatus)(rent.RentalStatus));
			if (ConverterEnumState.StateToBools(this.RentState, out bool IsReadyToReturn, out bool IsReturned))
			{
				this.IsReturned = IsReturned;
				this.IsReadyToReturn = IsReadyToReturn;
			}
		}
        public RentHistoryModel(ObceApi.RentalInfo rent, RentsRequest rr, int offerId, string platform)
        {
            this.Id = rent.Id;
            this.Client_Id = rr.Client_Id;
            this.Name = "no name";
            this.Surname = "no surname";
            this.Email = rr.Email;
            this.Platform = platform;
            this.RentDate = rent.RentDate;
            this.OfferId = offerId;
            this.IsReturned = false;
            this.IsReadyToReturn = false;

            this.RentState = ConverterEnumState.Convert((RentalStatus)(rent.RentalStatus));
			if(ConverterEnumState.StateToBools(this.RentState, out bool IsReadyToReturn, out bool IsReturned))
			{
				this.IsReturned = IsReturned;
				this.IsReadyToReturn = IsReadyToReturn;
			}
        }
    }

	public enum RentalStatus
	{
		Active = 0,
		Returned = 1,
		Closed = 2,
		not_Accepted = 3
	}

	public static class ConverterEnumState
	{
		public static Models.RentState Convert(RentalStatus status)
		{
			switch (status)
			{
				case RentalStatus.Active:
					return RentState.Active;
				case RentalStatus.not_Accepted:
					return RentState.In_Progess;
				case RentalStatus.Returned:
					return RentState.ReadyToReturn;
				case RentalStatus.Closed:
					return RentState.Returned;
				default:
					return RentState.Failure;
			};
		}
		public static bool StateToBools(RentState status, out bool IsReadyToReturn, out bool IsReturned)
		{
            switch (status)
            {
                case RentState.Active:
					IsReadyToReturn = false;
					IsReturned = false;
					return (true);
                case RentState.In_Progess:
					IsReadyToReturn = false;
					IsReturned = false;
					return (false);
				case RentState.Failure:
					IsReadyToReturn = false;
					IsReturned = false;
					return (false);
				case RentState.ReadyToReturn:
					IsReadyToReturn = true;
					IsReturned = false;
					return (true);
				case RentState.Returned:
					IsReadyToReturn = true;
					IsReturned = true;
					return (true);
				default:
					IsReadyToReturn = false;
					IsReturned = false;
					return (false);
			};
        }
	}



}
