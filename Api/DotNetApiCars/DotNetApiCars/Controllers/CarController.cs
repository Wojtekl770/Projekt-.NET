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

		[HttpPost]
		public void Post(Car car)
		{
			var c = new Car
			{
				Id = car.Id,
				CarBrand = car.CarBrand,
				CarModel = car.CarModel,
				IsRented = car.IsRented,
				LicensePlate = car.LicensePlate
			};

			_carContext.Cars.Add(c);
			_carContext.SaveChangesAsync();

		}


		[HttpPut]
		public async Task<Car?> Put(Car car)
		{
			//Car? _car = await _carContext.Cars.Where(c => c.Id == car.Id).FirstOrDefaultAsync();

			Car? _car = await _carContext.Cars.Where(c => (car.LicensePlate == c.LicensePlate)
														  && (car.CarModel == c.CarModel)
														  && (car.CarBrand == c.CarBrand))
														  .FirstOrDefaultAsync();
			if (_car == null)
				return null;

			if (!_car.IsRented)
			{
				_car.IsRented = true;
				await _carContext.SaveChangesAsync();
			}

			return _car;
		}

		[HttpPut]
		public async Task<Car?> Rent(int Client_Id, int Car_Id)
		{
			string? platform = HttpContext.Request.Headers["Origin"].ToString();

			Car? _car = await _carContext.Cars.Where(c => c.Id == Car_Id)
														  .FirstOrDefaultAsync();
			if (_car == null)
				return null;
			if (platform == null)
				return _car;


			if (!_car.IsRented)
			{
				_car.IsRented = true;
				_carContext.Rents.Add(new() { Client_Id = Client_Id, Platform = platform, Car = _car, RentDate = DateTime.Now });
				await _carContext.SaveChangesAsync();
			}


			return _car;
		}

		[HttpPut]
		public async Task<Car?> Unrent(Car car)
		{
			//Car? _car = await _carContext.Cars.Where(c => c.Id == car.Id).FirstOrDefaultAsync();

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
