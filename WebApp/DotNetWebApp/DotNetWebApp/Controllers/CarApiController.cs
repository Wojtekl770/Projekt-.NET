using DotNetWebApp.Data;
using DotNetWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebApp.Controllers
{

	public class CarApiController : Controller
	{
		private Uri baseAddress;
		private readonly HttpClient _client;
		private CarContext carContext;

		public CarApiController(CarContext carC)
		{
			string Uri = "https://localhost:7127/Car";
			Uri = "https://carrentalapi2-acg4cgdcanecabap.canadacentral-01.azurewebsites.net/Car";
			baseAddress = new Uri(Uri);
			_client = new HttpClient();
			_client.BaseAddress = baseAddress;
			carContext = carC;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return await Get();
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			List<Car> cars = new List<Car>();
			HttpResponseMessage? response = null;
			try
			{
				response = await _client.GetAsync(baseAddress + "/Get");
			}
			catch (HttpRequestException e)
			{
				return View(carContext.Cars.ToList());
			}

			if (response != null && response.IsSuccessStatusCode)
			{
				string data = await response.Content.ReadAsStringAsync();
				cars = JsonConvert.DeserializeObject<List<Car>>(data);

				if (carContext.Cars.Count() == 0)
					foreach (Car car in cars)
					{
						carContext.Add(car);
						await carContext.SaveChangesAsync();
					}

			}
			return View(carContext.Cars.ToList());
		}

		public async Task<IActionResult> Edit(int id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var car = carContext.Cars.Find(id);


			//access violation??????
			/*
			if (car == null)
			{
				return NotFound();
			}
			*/
			return View(car);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,LicensePlate,CarBrand,CarModel,IsRented")] Car car)
		{
			Car? _car = await carContext.Cars.Where(c => (car.LicensePlate == c.LicensePlate)
														  && (car.CarModel == c.CarModel)
														  && (car.CarBrand == c.CarBrand))
														  .FirstOrDefaultAsync();
			if (_car == null)
				return View(carContext);

			var jsonContent = JsonConvert.SerializeObject(car);
			var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _client.PutAsync(baseAddress + "/Put", httpContent);

			response.EnsureSuccessStatusCode();

			string data = await response.Content.ReadAsStringAsync();
			Car? car2 = JsonConvert.DeserializeObject<Car>(data);

			//if (_car != null && car2 != null)
			//{
			_car.IsRented = car2.IsRented;
			await carContext.SaveChangesAsync();
			//}

			return View(_car);

		}


	}
}
