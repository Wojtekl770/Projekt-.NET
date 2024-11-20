using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetApiCars.Controllers
{

	[ApiController]
	[Route("[controller]/[action]")]
	public class CarController : Controller
	{
		private readonly CarContext _carContext;
		public CarController(CarContext carContext)
		{
			_carContext = carContext;
		}

		[HttpGet]
		public async Task<IEnumerable<Car>> Get()
		{
			return await _carContext.Cars.ToListAsync();
		}
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


		[HttpPut]
		public async Task<int> Rent(OfferChoice oc)
		{
			OfferDB? _offer = await _carContext.OffersDB.Include(o => o.Car).Where(c => c.Id == oc.Offer_Id)
														  .FirstOrDefaultAsync();
			if (_offer == null)
				return -2;

			RentHistory history = new() { Id = -1};
			if (!_offer.Car.IsRented)
			{
				//wsm tutaj to powinno emaila wysylac a nie od razu na zawolanie wynajmowac

				_offer.Car.IsRented = true;

				history = new()
				{
					Name = oc.Name,
					Surname = oc.Surname,
					Email = oc.Email,
					Client_Id = oc.Client_Id,
					Platform = oc.Platform,
					OfferId = _offer.Id,
					Offer = _offer,
					RentDate = DateTime.Now
				};

				_carContext.Rents.Add(history);
				await _carContext.SaveChangesAsync();
			}


			return history.Id;
		}

		[HttpPost]
		public async Task<Offer?> CreateOffer(AskPrice ask)
		{
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
			catch(Exception)
			{
				return new() { IsSuccess = false, ExpirationDate = DateTime.Now, Id = -1, PriceDay = 0, PriceInsurance = 0 };
			}
		}

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


		/*
        // GET: CarController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CarController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CarController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CarController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CarController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CarController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
     */
	}

}
