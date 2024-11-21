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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public async Task<IActionResult> Search(string query)
        {
            await Get();

            if (string.IsNullOrEmpty(query))
            {
                // If no query is provided, redirect back to the Index view or return all cars
                return RedirectToAction("Index");
            }

            // Normalize query for case-insensitive search
            query = query.ToLower();

            // Perform search in the database
            var results = await carContext.Cars
                .Where(c => c.CarBrand.ToLower().Contains(query) || c.CarModel.ToLower().Contains(query))
                .GroupBy(c => new { c.CarBrand, c.CarModel })
                .Select(g => new CarOverall
                {
                    CarBrand = g.Key.CarBrand,
                    CarModel = g.Key.CarModel
                })
                .OrderBy(c => c.CarBrand)
                .ToListAsync();

            // Return search results to the same view
            return View("Index", results);
        }


        [HttpGet]
		public async Task<IActionResult> Get()
		{
			List<Car>? cars;
			HttpResponseMessage? response = null;
			try
			{
				response = await _client.GetAsync(baseAddress + "/Get");
			}
			catch (HttpRequestException e)
			{
				response = null;
			}

			if (response != null && response.IsSuccessStatusCode)
			{
				string data = await response.Content.ReadAsStringAsync();
				cars = JsonConvert.DeserializeObject<List<Car>>(data);
				List<Car> templist;

				if (cars != null)
				{
					//jezeli w pamieci podrecznej brak aut
					if (!carContext.Cars.Any())
						foreach (Car car in cars)
						{
							carContext.Add(car);
							await carContext.SaveChangesAsync();
						}
					else
						foreach (Car car in cars)
						{
							//istnialo auto wczesniej u nas
							if ((templist = (await carContext.Cars.ToListAsync()).Where(c => c.Id == car.Id).ToList()).Count != 0)
							{
								foreach (var c in templist)
									c.IsRented = car.IsRented;
							}
							else //nie istnialo auto wczesniej u nas
								carContext.Add(car);

							await carContext.SaveChangesAsync();
						}
				}

			}

			//wypluwamy widok modeli i marek
			List<CarOverall> co = [];
			foreach (var group in (await carContext.Cars.ToListAsync()).GroupBy(c => (c.CarBrand, c.CarModel)))
				co.Add(new() { CarBrand = group.Key.CarBrand, CarModel = group.Key.CarModel });

			co.Sort((c1, c2) => c1.CarBrand.CompareTo(c2.CarBrand));
			return View(co);
		}

		public async Task<IActionResult> Edit(int id)
		{
			var offer = (await carContext.Offers.Include(o => o.Car).ToListAsync()).Find(o => o.Id == id);

			if (offer == null)
				return NotFound();

			return View(offer);
		}

		public async Task<IActionResult> ShowOffers(CarOverall carOverall)
		{
			try
			{
				if (User.Identity == null || !User.Identity.IsAuthenticated)
					return Redirect("/Identity/Account/Login");

				if (carOverall == null)
					return NotFound();

				//auta dla tej marki i modelu
				List<Car> cars = (await carContext.Cars.ToListAsync()).FindAll(car =>
									car.CarModel == carOverall.CarModel && car.CarBrand == carOverall.CarBrand);


				//info do zlozenia oferty
				Random rand = new();
				int add = rand.Next() % 10;
				DateTime now = DateTime.Now;
				DateTime Start = now.AddDays(add);
				DateTime Return = now.AddDays(add + rand.Next() % 10);

				int yearOfBirth = now.Year;
				int yearNow = now.Year;
				int yearOfGettingDriversLicence = now.Year;

				if (int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "YearOfBirth")?.Value, out var yob))
					yearOfBirth = yob;
				if (int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "YearOfGettingDriversLicence")?.Value, out var yogdl))
					yearOfGettingDriversLicence = yogdl;



				ConcurrentBag<OfferCarModel> offerscars = [];
				ConcurrentBag<OfferCarModel> existingOffers = [];
				foreach (var m in (await carContext.Offers.Include(o => o.Car).ToListAsync())
					.Where(o => o.Car.CarModel == carOverall.CarModel && o.Car.CarBrand == carOverall.CarBrand))
				{
					//usuwamy niekatywne oferty
					if (m.ExpirationDate.CompareTo(now) < 0)
					{
						carContext.Offers.Remove(m);
						await carContext.SaveChangesAsync();
					}
					else //dodajemy do listy odpornej na async
						existingOffers.Add(m);
				}

				await Parallel.ForEachAsync(cars, async (car, cancell) =>
				{
					try
					{

						OfferCarModel? offerFromDatabase;
						if ((offerFromDatabase = existingOffers.Where(o => o.CarId == car.Id).FirstOrDefault()) != null)
						{
							//pokazujemy aktywna oferte z naszej lokalnej bazy danych
							offerscars.Add(offerFromDatabase);
						}
						else
						{
							//nowe zapytanie o oferte
							AskPrice ask = new()
							{
								Age = yearNow - yearOfBirth,
								DriversLicenceDuration = yearNow - yearOfGettingDriversLicence,
								Start = Start,
								Return = Return,
								ExtraInfo = "No Extra Info",
								Car_Id = car.Id
							};

							CancellationTokenSource cancel = new();
							cancel.CancelAfter(15000);

							//wysylamy oferte
							var response = await _client.PostAsJsonAsync(baseAddress + "/CreateOffer", ask, cancel.Token);
							response.EnsureSuccessStatusCode();

							string data = await response.Content.ReadAsStringAsync();
							Offer? offer = JsonConvert.DeserializeObject<Offer>(data);

							if (offer != null && offer.IsSuccess)
							{
								//dodajemy oferte na ten model
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
					}
					catch (Exception) { }
				});

				var final = offerscars.ToList();
				final.Sort((c1, c2)=>c1.CarId.CompareTo(c2.CarId));
				return View(final);
			}
			catch (Exception)
			{
				return Redirect("/CarApi/Index");
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id, CarId")] OfferCarID offerCarID/*[Bind("Id,LicensePlate,CarBrand,CarModel,IsRented Localization")] Car car*/)
		{
			try
			{
				if ((await carContext.Cars.ToListAsync()).Where(c => c.Id == offerCarID.CarId && c.IsRented).Count() > 0)
					return View((await carContext.Offers.ToListAsync()).First(o => offerCarID.Id == o.Id));


				//(int Client_Id, int Offer_Id)
				string platform = Request.Host.ToString();
				string id_client = "null";
				string? id_str = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (id_str != null)
					id_client = id_str;


				string? name2 = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
				string? surname2 = User.Claims.FirstOrDefault(c => c.Type == "Surname")?.Value;
				string? email2 = User.FindFirstValue(ClaimTypes.Email);

				string name = name2 ?? "";
				string surname = surname2 ?? "";
				string email = email2 ?? "";

				OfferChoice oc = new()
				{
					Client_Id = id_client,
					Offer_Id = id,
					Email = email,
					Name = name,
					Surname = surname,
					Platform = platform,
				};


				var response = await _client.PutAsJsonAsync(baseAddress + "/Rent", oc);
				response.EnsureSuccessStatusCode();
				string data = await response.Content.ReadAsStringAsync();
				int Rent_Id = int.Parse(data);//JsonConvert.DeserializeObject<int>(data);

				//if (Rent_Id != null)
				if (Rent_Id > -2)
				{
					//albo juz wynajety przez kogos innego == -1
					//albo udalo nam sie > 0

					//wsm to nic tu nie robimy, ale w api jakis email, cos?
					// no i trzeba zapisac ten rent_Id
					Car car = (await carContext.Cars.ToListAsync()).First(c => offerCarID.CarId == c.Id);
					car.IsRented = true;
					await carContext.SaveChangesAsync();
				}

				return View((await carContext.Offers.ToListAsync()).First(o => offerCarID.Id == o.Id));


			}
			catch (Exception e)
			{
				return Redirect("/CarApi/Index");
			}


		}


	}
}
