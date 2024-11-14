using Azure;
using DotNetWebApp.Data;
using DotNetWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Runtime.ConstrainedExecution;
using Microsoft.IdentityModel.Tokens;

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
			_client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
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
				//return View(carContext.Cars.ToList());

				List<CarOverall> co2 = [];
				foreach (var group in (await carContext.Cars.ToListAsync()).GroupBy(c => (c.CarBrand, c.CarModel)))
					co2.Add(new() { CarBrand = group.Key.CarBrand, CarModel = group.Key.CarModel });

				return View(co2);
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
			//return View(carContext.Cars.ToList());
			List<CarOverall> co = [];
			foreach (var group in (await carContext.Cars.ToListAsync()).GroupBy(c => (c.CarBrand, c.CarModel)))
				co.Add(new() { CarBrand = group.Key.CarBrand, CarModel = group.Key.CarModel });

			return View(co);
		}

		public async Task<IActionResult> Edit(int id)
		{
			/*
			if (id == null)
			{
				return NotFound();
			}

			var car = (await carContext.Cars.ToListAsync()).Find(c => c.Id == id);

			return View(car);
			*/

			if (id == null)
				return NotFound();

			var offer = (await carContext.Offers.Include(o => o.Car).ToListAsync()).Find(o => o.Id == id);

			return View(offer);
		}

		public async Task<IActionResult> ShowOffers(CarOverall carOverall)
		{
			try
			{
				if (User.Identity == null || !User.Identity.IsAuthenticated)
					return Redirect("/Identity/Account/Login");


				if (carOverall == null)
				{
					return NotFound();
				}

				var cars = (await carContext.Cars.ToListAsync()).FindAll(car =>
									car.CarModel == carOverall.CarModel && car.CarBrand == carOverall.CarBrand);


				Random rand = new();
				int add = rand.Next() % 10;
				DateTime Start = DateTime.Now.AddDays(add);
				DateTime Return = DateTime.Now.AddDays(add + rand.Next() % 10);

				int yearOfBirth = DateTime.Now.Year;
				int yearNow = DateTime.Now.Year;
				int yearOfGettingDriversLicence = DateTime.Now.Year;

				if (int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "YearOfBirth")?.Value, out var yob))
					yearOfBirth = yob;
				if (int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "YearOfGettingDriversLicence")?.Value, out var yogdl))
					yearOfGettingDriversLicence = yogdl;


				ConcurrentBag<OfferCarModel> offerscars = [];
				ConcurrentBag<OfferCarModel> existingOffers = [];
				foreach(var m in (await carContext.Offers.Include(o => o.Car).ToListAsync())
					.Where(o => o.Car.CarModel == carOverall.CarModel && o.Car.CarBrand == carOverall.CarBrand))
				{
					existingOffers.Add(m);
				}

				await Parallel.ForEachAsync(cars, async (car, cancell) =>
				{
					try
					{
						
						OfferCarModel? offerFromDatabase;
						if((offerFromDatabase = existingOffers.Where(o=> o.CarId == car.Id).FirstOrDefault()) != null)
						{
							offerscars.Add(offerFromDatabase);
							throw new Exception();
						}
						

						AskPrice ask = new()
						{
							Age = yearNow - yearOfBirth,
							DriversLicenceDuration = yearNow - yearOfGettingDriversLicence,
							Start = Start,
							Return = Return,
							ExtraInfo = "No Extra Info",
							Car_Id = car.Id
						};


						var response = await _client.PostAsJsonAsync(baseAddress + "/CreateOffer", ask);
						response.EnsureSuccessStatusCode();


						string data = await response.Content.ReadAsStringAsync();
						Offer? offer = JsonConvert.DeserializeObject<Offer>(data);

						if (offer.IsSuccess)
						{

							OfferCarModel ocm = new()
							{
								Id = offer.Id,
								PriceDay = offer.PriceDay,
								PriceInsurance = offer.PriceInsurance,
								ExpirationDate = offer.ExpirationDate,
								Car = car,
								CarId = car.Id
							};


							offerscars.Add(ocm);
							carContext.Offers.Add(ocm);
							await carContext.SaveChangesAsync();
						}
					}
					catch (Exception) { }
				});

				/*
				foreach(var o in offerscars)
				{
					carContext.Offers.Add(o);
					await carContext.SaveChangesAsync();
				}
				*/

				return View(offerscars.ToList());
			}
			catch (Exception)
			{
				return await Index();
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id, CarId")] OfferCarID offerCarID/*[Bind("Id,LicensePlate,CarBrand,CarModel,IsRented Localization")] Car car*/)
		{
			try
			{
				//(int Client_Id, int Offer_Id)
				int id_client = -1; //User.FindFirst(ClaimTypes.Email).Value;

				var requestMessage = new HttpRequestMessage(HttpMethod.Put, baseAddress + "/Rent" + $"?Client_Id={id_client}&Offer_Id={id}");
				requestMessage.Headers.Add("accept", "text/plain");
				var response = await _client.SendAsync(requestMessage);
				response.EnsureSuccessStatusCode();


				string data = await response.Content.ReadAsStringAsync();
				bool rented = JsonConvert.DeserializeObject<bool>(data);

				if(rented)
				{
					Car? _car = (await carContext.Cars.ToListAsync()).First(c => offerCarID.CarId == c.Id);

					_car.IsRented = true;
					await carContext.SaveChangesAsync();
				}


				OfferCarModel ofm = (await carContext.Offers.ToListAsync()).First(o => offerCarID.Id == o.Id);

				return View(ofm);
			}
			catch (Exception)
			{
				return View(id);
			}


			/*
			try
			{
				if (User.Identity == null || !User.Identity.IsAuthenticated)
					return Redirect("/Identity/Account/Login");


				Car? _car = (await carContext.Cars.ToListAsync()).First(c => (car.LicensePlate == c.LicensePlate)
															  && (car.CarModel == c.CarModel)
															  && (car.CarBrand == c.CarBrand));
				
				int id_client = -1; //User.FindFirst(ClaimTypes.Email).Value;


				var requestMessage = new HttpRequestMessage(HttpMethod.Put, baseAddress + "/Rent" + $"?Client_Id={id_client}&Car_Id={id}");
				requestMessage.Headers.Add("accept", "text/plain");
				var response = await _client.SendAsync(requestMessage);

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
			catch (Exception)
			{
				return View(id);
			}
			*/

		}


	}
}
