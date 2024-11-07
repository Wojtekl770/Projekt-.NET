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
        public IEnumerable<Car> Get()
        {
            return _carContext.Cars.ToList();
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
            _carContext.SaveChanges();

            if (ModelState.IsValid)
            {
                //_carContext.Add(c);
                //await _carContext.SaveChangesAsync();
            }
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
