using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DotNetApiCars.Controllers
{

	[ApiController]
	[Route("[controller]/[action]")]
	public class CarController : Controller
	{
		private readonly CarContext _carContext;
		private readonly IEmailSender _emailSender;  // Declare _emailSender
		private string apiKey = "some_random_key";
		private const string apiName = "X-Api-Key";

		// Inject IEmailSender into the constructor
		public CarController(CarContext carContext, IEmailSender emailSender)
		{
			_carContext = carContext;
			_emailSender = emailSender;  // Assign the injected IEmailSender
		}

		[HttpGet]
		public async Task<IEnumerable<Car>> Get([FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return [];

			return await _carContext.Cars.ToListAsync();
		}

		[HttpGet("{id}")]
		public ActionResult<Car?> GetCar(int id, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return NotFound();

			var car = _carContext.Cars.Find(id);

			if (car == null)
				return NotFound();

			return Ok(car);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<RentHistory?>> GetRent(int id, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return NotFound();

			var rent = _carContext.Rents.Find(id);

			if (rent == null)
				return NotFound();

			await NotConfirmedRent(id);

			return Ok(rent);
		}
		/*
		[HttpGet]
		public async Task<IEnumerable<RentHistory>> GetRents()
		{
			return (await _carContext.Rents.Include(o => o.Offer).ToListAsync());
		}


		[HttpPost]
		public void Post(Car car)
		{
			var c = new Car
			{
				Id = car.Id,
				CarBrand = car.CarBrand,
				CarModel = car.CarModel,
				IsRented = car.IsRented,
				LicensePlate = car.LicensePlate,
				Localization = car.Localization,
			};

			_carContext.Cars.Add(c);
			_carContext.SaveChangesAsync();

		}
		*/

		[HttpPut]
		public async Task<IEnumerable<RentHistory>> GetMyRents([FromBody] RentsRequest rr, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return [];

			IEnumerable<RentHistory>? rh = (await _carContext.Rents.Include(o => o.Offer).Include(r => r.Offer.Car).ToListAsync())
				.Where(r =>
				r.Client_Id == rr.Client_Id &&
				r.Platform == rr.Platform);

			return rh ?? [];
		}

		/*
		[HttpPut]
		public async Task<RentHistory?> GetRent(RentRequest rr)
		{
			RentHistory? rh =  (await _carContext.Rents.Include(o => o.Offer).Include(r => r.Offer.Car).ToListAsync())
				.Where(r =>
				r.Client_Id == rr.Client_Id &&
				r.Platform == rr.Platform &&
				r.Id == rr.Rent_Id).FirstOrDefault();

			return rh;
		}
		*/

		[HttpPut]
		[HttpPut]
		public async Task<int> Rent([FromBody] OfferChoice oc, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return -3;

			// Retrieve the offer and car
			OfferDB? _offer = await _carContext.OffersDB.Include(o => o.Car)
														 .Where(c => c.Id == oc.Offer_Id)
														 .FirstOrDefaultAsync();

			if (_offer == null)
				return -2;

			RentHistory history = new() { Id = -1 };

			// If the car is not rented
			if (!_offer.Car.IsRented)
			{
				// Mark the car as rented
				_offer.Car.IsRented = true;

				history = new RentHistory
				{
					Name = oc.Name,
					Surname = oc.Surname,
					Email = oc.Email,
					Client_Id = oc.Client_Id,
					Platform = oc.Platform,
					OfferId = _offer.Id,
					Offer = _offer,
					RentDate = DateTime.Now,
					//RentState = RentState.Active
					RentState = RentState.In_Progess
				};

				// Save the rent history
				_carContext.Rents.Add(history);
				await _carContext.SaveChangesAsync();

				//DODAJ PO SKONCZONYCH TESTACH
				/*
				string? confirmationLink = Url.Action("ConfirmRent", "Car",
	               new { id = history.Id }, protocol: HttpContext.Request.Scheme);
				string? x = confirmationLink;
				
				// Email subject and body
				var emailSubject = "Car Rental Confirmation";
				var emailBody = $"Dear {oc.Name},<br><br>" +
								$"You want to rent the car: <strong>{_offer.Car.CarBrand} {_offer.Car.CarModel}</strong>.<br>" +
								$"Please confirm your rental by clicking the link below:<br><br>" +
								$"<a href='{confirmationLink}' target='_blank'>Confirm Rental</a><br><br>" +
								$"Thank you for using our service!<br><br>" +
								$"Best regards,<br>The Car Rental Team";

				// Send email to the user (the email in the request)
				await _emailSender.SendEmailAsync(oc.Email, emailSubject, emailBody);  // Send email to the user
				*/

				await ConfirmRent(history.Id);

				var t = Task.Run(async delegate
				{
					await Task.Delay(TimeSpan.FromSeconds(60));
					await NotConfirmedRent(history.Id);
				});
				//t.Wait();


			}

			return history.Id;
		}

		[HttpPut("{id}")]
		public async Task<int> ConfirmRent(int id /*,[FromHeader(Name = apiName)] string apiKey*/)
		//public async Task<int> ConfirmRent(int id, string apiKey)
		{
			//if (apiKey != this.apiKey)
				//return -3;

			var rent = _carContext.Rents.Find(id);

			if (rent == null)
				return -3;

			if (rent.RentState == RentState.In_Progess)
			{
				rent.RentState = RentState.Active;

				await _carContext.SaveChangesAsync();

				//DODAJ PO SKONCZONYCH TESTACH
				/*
                // Send the rental confirmation email
                var emailSubject = "Car Rental Confirmation";
                var emailBody = $"Dear {oc.Name},<br><br>" +
                                $"You have successfully rented the car: <strong>{_offer.Car.CarBrand} {_offer.Car.CarModel}</strong>.<br>" +
                                $"Thank you for using our service!<br><br>" +
                                $"Best regards,<br>The Car Rental Team";

                // Send email to the user (the email in the request)
                await _emailSender.SendEmailAsync(oc.Email, emailSubject, emailBody);  // Send email to the user
				*/
			}


			return id;
		}


		private async Task<int> NotConfirmedRent(int id)
		{
			var rent = _carContext.Rents.Find(id);

			if (rent == null)
				return -3;

			if(rent.Offer == null)
			{
				var offer = _carContext.OffersDB.Find(rent.OfferId);

				if (offer == null)
					return -3;

                rent.Offer = offer;
			}

			if (rent.Offer.Car == null)
			{
				var car = _carContext.Cars.Find(rent.Offer.CarId);

				if (car == null)
					return -3;

				rent.Offer.Car = car;
			}

			if (rent.RentState == RentState.In_Progess)
			{
				rent.RentState = RentState.Failure;
				rent.IsReturned = true;
				rent.IsReadyToReturn = true;
				rent.Offer.Car.IsRented = false;
			}

			await _carContext.SaveChangesAsync();


			return id;
		}



		[HttpPost]
		public async Task<Offer?> CreateOffer([FromBody] AskPrice ask, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return new();

			Console.WriteLine(apiKey);
			try
			{
				if (ask.Age < 18)
					return new() { IsSuccess = false, ExpirationDate = DateTime.Now, Id = -1, PriceDay = 0, PriceInsurance = 0 };

				if (!(await _carContext.Cars.Where(c => c.Id == ask.Car_Id).AnyAsync()))
					return new() { IsSuccess = false, ExpirationDate = DateTime.Now, Id = -1, PriceDay = 0, PriceInsurance = 0 };

				Car car = await _carContext.Cars.Where(c => c.Id == ask.Car_Id).FirstAsync();

				Random rand = new();

				DateTime date = DateTime.Now;
				Offer offer = new()
				{
					IsSuccess = true,
					ExpirationDate = date.AddHours(1 + rand.Next() % 10),
					//Id = -1,
					PriceDay = 100 + rand.Next() % 200,
					PriceInsurance = 100 + rand.Next() % 200
				};

				_carContext.OffersDB.Add(new()
				{
					PriceDay = offer.PriceDay,
					PriceInsurance = offer.PriceInsurance,
					ExpirationDate = offer.ExpirationDate,
					WhenOfferWasMade = date,
					Car = car,
					CarId = car.Id,
				});

				await _carContext.SaveChangesAsync();

				offer.Id = _carContext.OffersDB.Where(o => (o.WhenOfferWasMade == date
											 && o.CarId == car.Id
											 && o.ExpirationDate == offer.ExpirationDate
											 && o.PriceInsurance == offer.PriceInsurance)).First().Id;

				return offer;
			}
			catch (Exception)
			{
				return new() { IsSuccess = false, ExpirationDate = DateTime.Now, Id = -1, PriceDay = 0, PriceInsurance = 0 };
			}
		}
		/*
		[HttpPut]
		public async Task<Car?> Unrent(Car car)
		{
			Car? _car = await _carContext.Cars.Where(c => (car.LicensePlate == c.LicensePlate)
														  && (car.CarModel == c.CarModel)
														  && (car.CarBrand == c.CarBrand))
														  .FirstOrDefaultAsync();
			if (_car == null)
				return null;

			if (_car.IsRented)
			{
				_car.IsRented = false;
				await _carContext.SaveChangesAsync();
			}

			return _car;
		}
		*/
		[HttpPut]
		public async Task<bool> Return([FromBody] ReturnRequest retr, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return false;

			RentHistory? rh = (await _carContext.Rents.Include(o => o.Offer).Include(r => r.Offer.Car).ToListAsync())
				.Where(r =>
				r.Client_Id == retr.Client_Id &&
				r.Platform == retr.Platform &&
				r.Id == retr.Rent_Id).FirstOrDefault();

			if (rh == null)
				return false;

			//dodac emaila
			//rh.IsReturned = true;
			//rh.Offer.Car.IsRented = false;
			rh.RentState = RentState.ReadyToReturn;
			rh.IsReadyToReturn = true;
			await _carContext.SaveChangesAsync();


			return true;
		}

		[HttpPost]
		public async Task<bool> ConfirmReturn([FromBody] ReturnRequest retr, [FromHeader(Name = apiName)] string apiKey)
		{
			if (apiKey != this.apiKey)
				return false;

			RentHistory? rh = (await _carContext.Rents.Include(o => o.Offer).Include(r => r.Offer.Car).ToListAsync())
				.Where(r =>
				r.Client_Id == retr.Client_Id &&
				r.Platform == retr.Platform &&
				r.Id == retr.Rent_Id).FirstOrDefault();

			if (rh == null)
				return false;

			if (!rh.IsReadyToReturn)
				return false;

			//dodac emaila
			rh.RentState = RentState.Returned;
			rh.IsReturned = true;
			rh.Offer.Car.IsRented = false;
			await _carContext.SaveChangesAsync();


			int days = ((DateTime.Now - rh.RentDate).Days);
			if (days < 0)
				days = 0;

			int totalCost = days * rh.Offer.PriceDay + rh.Offer.PriceInsurance;


			// Send the rental confirmation email
			var emailSubject = "Car Rental Confirmation";
			var emailBody = $"Dear Customer,<br><br>" +
							$"You have successfully returned the car: <strong>{rh.Offer.Car.CarBrand} {rh.Offer.Car.CarModel}</strong>.<br>" +
							$"Your Total Cost: {totalCost} euro<br><br>" +
							$"Thank you for using our service!<br><br>" +
							$"Best regards,<br>The Car Rental Team";

			// Send email to the user (the email in the request)
			await _emailSender.SendEmailAsync(retr.Email, emailSubject, emailBody);  // Send email to the user

			return true;
		}

	}



}
