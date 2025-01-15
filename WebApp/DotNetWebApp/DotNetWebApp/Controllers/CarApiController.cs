using Azure;
using DotNetWebApp.Data;
using DotNetWebApp.Models;
using DotNetWebApp.ObceApi;
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
using System.Drawing;
using System.Linq;
//using AspNetCore;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.WebRequestMethods;
using Azure.Storage.Blobs;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Reflection.PortableExecutable;

namespace DotNetWebApp.Controllers
{

	public class CarApiController : Controller
	{
		private string Uri, Uri2;
		//private Uri baseAddress;
		private readonly HttpClient _client, _client2;
		private CarContext carContext;
		private readonly BlobStorageService _blobStorageService;
		private string apiKey = "some_random_key";
		private string apiName = "X-Api-Key";
		private string apiKey2 = "5faa0775-1e65-4616-9974-4922ec588269";
		private string clientName = "X-Client-Id";
		private string clientKey2 = "ClientId01";
		public CarApiController(CarContext carC)
		{
			//string Uri = "https://localhost:7127/Car";
			string Uri = "https://webapp2net-gmd6bjgfggduhqf0.polandcentral-01.azurewebsites.net/Car";
			string Uri2 = "https://minicarrentalapi.azurewebsites.net/api";

			this.Uri = Uri;
			Uri baseAddress = new Uri(this.Uri);
			_client = new HttpClient();
			_client.BaseAddress = baseAddress;
			_client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
			_client.DefaultRequestHeaders.Add(apiName, apiKey);

			this.Uri2 = Uri2;
			Uri baseAddress2 = new Uri(this.Uri2);
			_client2 = new HttpClient();
			_client2.BaseAddress = baseAddress2;
			_client2.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
			_client2.DefaultRequestHeaders.Add(apiName, apiKey2);
			_client2.DefaultRequestHeaders.Add(clientName, clientKey2);

			carContext = carC;

			_blobStorageService = new();
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			await GetThemAll();

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
		public async Task<bool> GetThemAll()
		{
			List<ObceApi.Car2>? cars;
			HttpResponseMessage? response = null;
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, _client2.BaseAddress + "/Cars/allAvailable");
				response = await _client2.SendAsync(request);
			}
			catch (HttpRequestException e)
			{
				response = null;
				return false;
			}

			string data = await response.Content.ReadAsStringAsync();
			cars = JsonConvert.DeserializeObject<List<ObceApi.Car2>>(data);
			if (cars == null)
				return false;

			ConcurrentBag<int> ids = new((await carContext.Cars.Where(c => c.Platform == Uri2).ToListAsync()).Select(c => c.Id));


			await Parallel.ForEachAsync(cars, async (car, cancell) =>
			//foreach (ObceApi.Car2 car in cars)
			{
				if (!ids.Contains(car.Id + 100)) //nie istnialo auto wczesniej u nas
				{
					string localization = "unknown";
					string licenceplate = "XYZ000";

					lock (carContext)
					{
						carContext
							.Add(new CarPlatform()
							{
								Id = car.Id + 100,
								CarBrand = car.BrandName,
								CarModel = car.ModelName,
								LicensePlate = licenceplate,
								IsRented = false,
								Localization = localization,
								Platform = Uri2
							}
							);

						carContext.SaveChanges();
					}
				}
			}
			);

			return true;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			List<Models.Car>? cars;
			HttpResponseMessage? response = null;
			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, _client.BaseAddress + "/Get");
				response = await _client.SendAsync(request);
			}
			catch (HttpRequestException e)
			{
				response = null;
			}

			if (response != null && response.IsSuccessStatusCode)
			{
				string data = await response.Content.ReadAsStringAsync();
				cars = JsonConvert.DeserializeObject<List<Models.Car>>(data);
				List<CarPlatform> templist;

				if (cars != null)
				{
					//jezeli w pamieci podrecznej brak aut
					if (!carContext.Cars.Any())
						foreach (Models.Car car in cars)
						{
							CarPlatform new_car = new()
							{
								Id = car.Id,
								CarBrand = car.CarBrand,
								CarModel = car.CarModel,
								LicensePlate = car.LicensePlate,
								IsRented = car.IsRented,
								Localization = car.Localization,
								Platform = Uri
							};
							carContext.Add(new_car);
							await carContext.SaveChangesAsync();
						}
					else
						foreach (Models.Car car in cars)
						{
							//istnialo auto wczesniej u nas
							if ((templist = (await carContext.Cars.ToListAsync()).Where(c => c.Id == car.Id && c.Platform == Uri).ToList()).Count != 0)
							{
								foreach (var c in templist)
									c.IsRented = car.IsRented;
							}
							else //nie istnialo auto wczesniej u nas
								carContext
									.Add(new CarPlatform()
									{
										Id = car.Id,
										CarBrand = car.CarBrand,
										CarModel = car.CarModel,
										LicensePlate = car.LicensePlate,
										IsRented = car.IsRented,
										Localization = car.Localization,
										Platform = Uri
									}
									);

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

		public async Task<IActionResult> Edit(int id, string Platform)
		{
			var offer = (await carContext.Offers.Include(o => o.Car).ToListAsync()).Find(o => o.Id == id && o.Platform == Platform);

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
				List<CarPlatform> cars = (await carContext.Cars.ToListAsync()).FindAll(car =>
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
				ConcurrentBag<OfferCarModel> existingOrUsedOffers = [];
				foreach (var m in (await carContext.Offers.Include(o => o.Car).ToListAsync())
					.Where(o => o.Car.CarModel == carOverall.CarModel && o.Car.CarBrand == carOverall.CarBrand))
				{
					//usuwamy niekatywne oferty (pomijajac w ususwaniu nieaktywna oferty wykorzystane bedace w rents)
					if (m.ExpirationDate.CompareTo(now) < 0
						&& !(await carContext.Rents.Where(re => re.OfferId == m.Id && re.Platform == m.Platform).ToListAsync()).Any())
					{
						m.Car = null;
						carContext.Offers.Remove(m);
						await carContext.SaveChangesAsync();
					}
					else //dodajemy do listy odpornej na async
						existingOrUsedOffers.Add(m);
				}


				await Parallel.ForEachAsync(cars, async (car, cancell) =>
				{
					if (car.Platform == Uri)
					{
						try
						{
							OfferCarModel? offerFromDatabase;
							if ((offerFromDatabase = existingOrUsedOffers
								.Where(o => o.CarId == car.Id && o.Platform == car.Platform && o.ExpirationDate.CompareTo(now) < 0)
								.FirstOrDefault()) != null)
							{
								//pokazujemy AKTYWNA oferte z naszej lokalnej bazy danych
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
								var response = await _client.PostAsJsonAsync(_client.BaseAddress + "/CreateOffer", ask, cancel.Token);
								/*
								string jsonContent = System.Text.Json.JsonSerializer.Serialize(ask);
								var request = new HttpRequestMessage(HttpMethod.Post, _client.BaseAddress + "/CreateOffer");
								request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
								var response = await _client.SendAsync(request, cancel.Token);
								*/
								response.EnsureSuccessStatusCode();

								string data = await response.Content.ReadAsStringAsync();
								Models.Offer? offer = JsonConvert.DeserializeObject<Models.Offer>(data);

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
										CarId = car.Id,
										Platform = Uri
									};

									offerscars.Add(ocm);
									lock (carContext)
									{
										carContext.Offers.Add(ocm);
										carContext.SaveChanges();
									}
								}
							}
						}
						catch (Exception) { }
					}
					else if (car.Platform == Uri2)
					{
						List<OfferCarModel> offersFromDatabase;
						if ((offersFromDatabase = existingOrUsedOffers
							.Where(o => o.CarId == car.Id && o.Platform == car.Platform && o.ExpirationDate.CompareTo(now) < 0).ToList()).Any())
						{
							//pokazujemy AKTYWNE oferty z naszej lokalnej bazy danych
							foreach (var o in offersFromDatabase)
								offerscars.Add(o);
						}
						else
						{
							if (car.Localization == "unknown")
							{
								car = await AddCar(car, car.Id, false);
							}


							HttpResponseMessage? response = null;
							try
							{
								var request = new HttpRequestMessage(HttpMethod.Get, _client2.BaseAddress + "/Rental/offers/" + (car.Id - 100));
								response = await _client2.SendAsync(request);
								response.EnsureSuccessStatusCode();
							}
							catch (HttpRequestException e)
							{
								return;
							}

							string data = await response.Content.ReadAsStringAsync();
							List<ObceApi.Offer>? offers = JsonConvert.DeserializeObject<List<ObceApi.Offer>>(data);


							if (offers != null)
								foreach (var offer in offers)
								{
									//dodajemy oferte na ten model
									OfferCarModel ocm = new()
									{
										Id = offer.Id,
										PriceDay = Decimal.ToInt32(offer.Price),
										PriceInsurance = offer.IsInsurance ? Decimal.ToInt32(offer.Price) : 0,
										ExpirationDate = offer.ExpirationDate,
										Car = car,
										CarId = car.Id,
										Platform = Uri2,
										GUID = offer.OfferGuid.ToString()
									};

									offerscars.Add(ocm);
									lock (carContext)
									{
										carContext.Offers.Add(ocm);
										carContext.SaveChanges();
									}
								}
						}
					}
				});

				var final = offerscars.ToList();
				final.Sort((c1, c2) => c1.CarId.CompareTo(c2.CarId));
				return View(final);
			}
			catch (Exception e)
			{
				return Redirect("/CarApi/Index");
			}
		}

		private async Task<CarPlatform?> AddCar(CarPlatform? car, int CarId, bool createCar)
		{
			try
			{
				string localization = "unknown";
				string licenceplate = "XYZ000";

				var request = new HttpRequestMessage(HttpMethod.Get, _client2.BaseAddress + "/Cars/" + (CarId - 100));
				HttpResponseMessage? response2 = await _client2.SendAsync(request);

				string data2 = await response2.Content.ReadAsStringAsync();
				ObceApi.Car? carFull = JsonConvert.DeserializeObject<ObceApi.Car>(data2);


				if (carFull != null)
				{
					if (createCar)
					{
						car = new()
						{
							Id = CarId,
							Platform = Uri2,
							CarBrand = carFull.Model.Brand.Name,
							CarModel = carFull.Model.Name,
							//LicensePlate = licenceplate,
							IsRented = false,
							//Localization = localization
						};

						carContext.Add(car);
						await carContext.SaveChangesAsync();
					}

					if (carFull.Location != null)
					{
						localization = $"Lat: {carFull.Location.Latitude}, Long: {carFull.Location.Longitude}";
						licenceplate = $"{carFull.FuelType.Substring(0, 2)}{carFull.Colour.Substring(1, 1)}{carFull.DoorsNumber}{carFull.HorsePower}";

						lock (carContext)
						{
							car.Localization = localization;
							car.LicensePlate = licenceplate;
							carContext.SaveChanges();
						}
					}
				}

			}
			catch (HttpRequestException e) { }
			return car;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id, CarId, Platform")] OfferCarID offerCarID/*[Bind("Id,LicensePlate,CarBrand,CarModel,IsRented Localization")] Car car*/)
		{
			try
			{
				//o co biega????
				if ((await carContext.Cars.ToListAsync()).Where(c => c.Id == offerCarID.CarId && c.IsRented && c.Platform == offerCarID.Platform).Count() > 0)
					return View(carContext.Offers.Where(o => offerCarID.Id == o.Id && o.Platform == offerCarID.Platform).First());


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

				
				if (offerCarID.Platform == Uri)
				{
					var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/Rent", oc);
					/*
					string jsonContent = System.Text.Json.JsonSerializer.Serialize(oc);
					var request = new HttpRequestMessage(HttpMethod.Put, _client.BaseAddress + "/Rent");
					request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
					var response = await _client.SendAsync(request);
					*/
					response.EnsureSuccessStatusCode();

					string data = await response.Content.ReadAsStringAsync();
					int Rent_Id = int.Parse(data);

					//if (Rent_Id != null)
					if (Rent_Id > -2)
					{
						//albo juz wynajety przez kogos innego == -1
						//albo udalo nam sie > 0

						//wsm to nic tu nie robimy, ale w api jakis email, cos?
						// no i trzeba zapisac ten rent_Id
						CarPlatform car = (await carContext.Cars.ToListAsync()).First(c => offerCarID.CarId == c.Id && c.Platform == offerCarID.Platform);
						car.IsRented = true;
						await carContext.SaveChangesAsync();
					}
				}
				else if (offerCarID.Platform == Uri2)
				{
					//string jsonContent = "{\"emailAddress\": \"" + email + "\"}"; //System.Text.Json.JsonSerializer.Serialize(email);
					string jsonContent = "\"" + email + "\"";
					var request = new HttpRequestMessage(HttpMethod.Put, _client2.BaseAddress + "/Rental/offers/chooseOffer/" + offerCarID.Id);
					request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
					var response = await _client2.SendAsync(request);
					response.EnsureSuccessStatusCode();

					string data = await response.Content.ReadAsStringAsync();
					ObceApi.RentalInfo? rent = JsonConvert.DeserializeObject<ObceApi.RentalInfo>(data);

					if (rent != null)
					{
						Models.RentHistoryModel rent_correct = new(rent, oc, Uri2);

						CarPlatform car = (await carContext.Cars.ToListAsync()).First(c => offerCarID.CarId == c.Id && c.Platform == offerCarID.Platform);
						car.IsRented = true;
						await carContext.SaveChangesAsync();

						carContext.Rents.Add(rent_correct);
						await carContext.SaveChangesAsync();
					}

				}

				return View((await carContext.Offers.ToListAsync()).First(o => offerCarID.Id == o.Id && o.Platform == offerCarID.Platform));


			}
			catch (Exception e)
			{
				return Redirect("/CarApi/Index");
			}


		}

		public async Task<IActionResult> MyRents()
		{
			string platform = Request.Host.ToString();
			string id_client = "null";
			string? id_str = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (id_str != null)
				id_client = id_str;

			string? email2 = User.FindFirstValue(ClaimTypes.Email);
			string email = email2 ?? "";

			RentsRequest rr = new() { Client_Id = id_client, Email = email, Platform = platform };

			await Rents(rr, false);

			var rents = await carContext.Rents.Include(r => r.Offer).Include(r => r.Offer.Car).Where(r => r.Client_Id == id_client).ToListAsync();

			return View(rents);

		}

		private async Task<int> RentsThemAll(RentsRequest rr)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, _client2.BaseAddress + "/Rental/rentals/allRentals/" + rr.Email);
			var response = await _client2.SendAsync(request);

			response.EnsureSuccessStatusCode();
			var data = await response.Content.ReadAsStringAsync();
			var rentsRaw = JsonConvert.DeserializeObject<IEnumerable<ObceApi.RentalInfo>>(data);

			if (rentsRaw != null)
			{
				RentHistoryModel? rhm = null;
				foreach (var rentRaw in rentsRaw)
					if ((rhm = await carContext.Rents.Where(r => r.Id == rentRaw.Id && r.Platform == Uri2).FirstOrDefaultAsync()) == null)
					{
						OfferCarModel? offer;
						if ((offer = await carContext.Offers.Where(o => o.GUID == rentRaw.OfferGuid).FirstOrDefaultAsync()) == null)
						{
							offer = new()
							{
								Platform = Uri2,
								PriceDay = (int)(rentRaw.PricePerDay),
								PriceInsurance = rentRaw.IsInsurance ? (int)(rentRaw.PricePerDay) : 0,
								ExpirationDate = DateTime.Now.AddMinutes(1),
								CarId = rentRaw.CarID + 100,
								GUID = rentRaw.OfferGuid
							};

							carContext.Add(offer);
							await carContext.SaveChangesAsync();


							offer.Car = await carContext.Cars.Where(c => c.Id == offer.CarId && c.Platform == Uri2).FirstOrDefaultAsync();
							await carContext.SaveChangesAsync();
							if (!await carContext.Cars.Where(c => c.Id == offer.CarId && c.Platform == Uri2).AnyAsync())
							{
								CarPlatform? car = null;
								car = await AddCar(car, offer.CarId, true);
								offer.Car = car;
								await carContext.SaveChangesAsync();
							}
							else
							{
								offer.Car = await AddCar(offer.Car, offer.Car.Id, false);
								await carContext.SaveChangesAsync();
							}


							offer = await carContext.Offers.Where(o => o.GUID == rentRaw.OfferGuid).FirstOrDefaultAsync();
						}

						RentHistoryModel rent = new(rentRaw, rr, offer.Id, Uri2);
						carContext.Add(rent);
						await carContext.SaveChangesAsync();

						rent.Offer = offer;
						await carContext.SaveChangesAsync();

					}
					else
					{
						rhm.RentState = ConverterEnumState.Convert((RentalStatus)rentRaw.RentalStatus);
						if (ConverterEnumState.StateToBools(rhm.RentState, out bool IsReadyToReturn, out bool IsReturned))
						{
							rhm.IsReadyToReturn = IsReadyToReturn;
							rhm.IsReturned = IsReturned;
						}
						await carContext.SaveChangesAsync();
					}

			}
			return 0;
		}

		[HttpPut]
		private async Task<int> Rents(RentsRequest rr, bool onlyNotReturned)
		{
			try
			{
				if (onlyNotReturned)
					//ususwanie zwroconych wynajmow - wsm tu sie nic nie dzieje xd - no a po poprawkach potrzebane jest jak sie okazuje
					foreach (RentHistoryModel rh in (await carContext.Rents.ToListAsync()))
						if (rh.IsReturned)
						{
							//rh.Offer.Car = null;
							rh.Offer = null;
							carContext.Remove(rh);
							await carContext.SaveChangesAsync();
						}

				var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/GetMyRents", rr);

				response.EnsureSuccessStatusCode();
				string data = await response.Content.ReadAsStringAsync();
				IEnumerable<RentHistory>? rents = JsonConvert.DeserializeObject<IEnumerable<RentHistory>>(data);


				/**/
				/**/
				await RentsThemAll(rr);
				/**/
				/**/


				if (rents == null)
					return -1;

				foreach (RentHistory rent in rents)
				{
					RentHistoryModel? temprent;
					RentHistoryModel rentModel = new(rent, Uri);
					var x = rentModel.IsReturned;

					//dodanie takich ktorych nie ma w naszej lokalnej DB i nie sa zwrocone (lub chcemy tez nie zwrocone)
					bool b = (temprent = (await carContext.Rents.ToListAsync()).Where(r => r.Id == rentModel.Id && r.Platform == rentModel.Platform).FirstOrDefault()) == null && (!onlyNotReturned || !rentModel.IsReturned);
					if (b)
					{
						carContext.Add(rentModel);
						await carContext.SaveChangesAsync();


						//dodanie referencji na auto i oferte
						rentModel.Offer = (await carContext.Offers.ToListAsync()).FirstOrDefault(o => o.Id == rentModel.OfferId && o.Platform == rentModel.Platform);
						await carContext.SaveChangesAsync();
						if (rentModel.Offer == null)
						{
							rentModel.Offer = new(rent.Offer, Uri);
							carContext.Add(rentModel.Offer);
							await carContext.SaveChangesAsync();


							//nie POWINNO byc problemyu z dodaniem auta
							//if (rentModel.Offer.Car == null)
							//{
							rentModel.Offer.Car = (await carContext.Cars.ToListAsync()).FirstOrDefault(c => c.Id == rentModel.Offer.CarId && c.Platform == rentModel.Platform);
							await carContext.SaveChangesAsync();
							//}
						}

					}
					else if (temprent != null)
					{
						temprent.RentState = rent.RentState;
						await carContext.SaveChangesAsync();

						//jezeli jest w bazie danych ale trzeba usunac ja bo juz zwrocilismy (chyba ze chcemy te zwrocone juz ogladac)
						if (onlyNotReturned && rentModel.IsReturned)
						{
							temprent.Offer = null;
							carContext.Remove(temprent);
							await carContext.SaveChangesAsync();
						}

					}


				}


			}
			catch (Exception e)
			{
				string s = e.Message;
			}

			return 0;
		}

		public async Task<IActionResult> Return(RentHistory rh) //tylko id poprawne tutaj
		{
			string platform = Request.Host.ToString();
			string id_client = "null";
			string? id_str = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (id_str != null)
				id_client = id_str;

			string? email2 = User.FindFirstValue(ClaimTypes.Email);
			string email = email2 ?? "";

			try
			{
				if (rh.Platform == Uri)
				{
					ReturnRequest rr = new() { Client_Id = id_client, Email = email, Platform = platform, Rent_Id = rh.Id };

					//proba zwrotu?
					var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/Return", rr);
					/*
					string jsonContent = System.Text.Json.JsonSerializer.Serialize(rr);
					var request = new HttpRequestMessage(HttpMethod.Put, _client.BaseAddress + "/Return");
					request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
					var response = await _client.SendAsync(request);
					*/

					response.EnsureSuccessStatusCode();
					string data = await response.Content.ReadAsStringAsync();
					bool? success = JsonConvert.DeserializeObject<bool>(data);

					if (success != null && (bool)success)
					{
						IEnumerable<RentHistoryModel> temprents = (await carContext.Rents
							.Include(r => r.Offer)
							.Include(r => r.Offer.Car)
							.ToListAsync()).Where(r => r.Id == rh.Id && r.Platform == rh.Platform);

						if (temprents.Count() == 1)
						{
							RentHistoryModel? rh2 = temprents.FirstOrDefault();
							if (rh2 != null)
							{
								rh2.IsReadyToReturn = true;

								await carContext.SaveChangesAsync();
							}
						}

					}
				}
				else if (rh.Platform == Uri2)
				{

					var response = await _client2.PutAsJsonAsync(_client2.BaseAddress + "/Rental/rentals/returnCar/" + rh.Id, email);
					response.EnsureSuccessStatusCode();
					string data = await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception e)
			{
				string x = e.Message;
			}


			return Redirect("/CarApi/MyRents");
		}

		public async Task<IActionResult> AllRents()
		{
			//jakos wykorzystaj te poprezdnie funkcje
			string platform = Request.Host.ToString();
			Uri uri = new("https://" + platform);
			HttpClient client = new() { BaseAddress = uri };

			var response = await client.GetAsync(uri + "Home/GetUsers");
			response.EnsureSuccessStatusCode();
			string data = await response.Content.ReadAsStringAsync();
			IEnumerable<CustomUser>? users = JsonConvert.DeserializeObject<IEnumerable<CustomUser>>(data);

			if (users != null)
				foreach (CustomUser u in users)
				{
					RentsRequest rr = new() { Client_Id = u.Id, Email = u.Email ?? "", Platform = platform };
					await Rents(rr, false);
				}



			return View(await carContext.Rents.Include(r => r.Offer).Include(r => r.Offer.Car).ToListAsync());
		}

		public async Task<IActionResult> ConfirmReturn(RentHistory rh) //tylko id poprawne tutaj i platform
		{
			try
			{
				RentHistoryModel rhm = (await carContext.Rents.Where(r => rh.Id == r.Id && rh.Platform == r.Platform)
					.Include(r => r.Offer)
					.Include(r => r.Offer.Car)
					.ToListAsync()).First();

				return View(rhm);
			}
			catch (Exception)
			{
				return Redirect("/CarApi/AllRents");
			}


		}

		[HttpPost]
		public async Task<IActionResult> ConfirmReturnFunction(ConfirmReturn rh)
		{
			string platform = Request.Host.ToString();

			ReturnRequest rcr = new() { Client_Id = rh.Client_Id, Email = rh.Email, Platform = platform, Rent_Id = rh.Id };

			RentHistoryModel? rent = carContext.Rents.Find(rh.Id);

			if (rent.Platform == Uri)
			{
				if (rh.IsReadyToReturn)
					try
					{
						//proba zwrotu?
						var response = await _client.PostAsJsonAsync(_client.BaseAddress + "/ConfirmReturn", rcr);
						
						response.EnsureSuccessStatusCode();
						string data = await response.Content.ReadAsStringAsync();
						bool? success = JsonConvert.DeserializeObject<bool>(data);

						if (success != null && (bool)success)
						{
							IEnumerable<RentHistoryModel> temprents = (await carContext.Rents
								.Include(r => r.Offer)
								.Include(r => r.Offer.Car)
								.ToListAsync()).Where(r => r.Id == rh.Id && r.Platform == rent.Platform);

							if (temprents.Count() == 1)
							{
								RentHistoryModel? rh2 = temprents.FirstOrDefault();
								if (rh2 != null)
								{
									if (rh.File != null)
										await UploadImage(rh.File);

									rh2.IsReturned = true;
									rh2.Offer.Car.IsRented = false;

									await carContext.SaveChangesAsync();
								}
							}

						}
					}
					catch (Exception e)
					{
						string x = e.Message;
					}
			}
			else if (rent.Platform == Uri2)
			{
				if (rh.IsReadyToReturn)
					try
					{
						IEnumerable<RentHistoryModel> temprents = (await carContext.Rents
								.Include(r => r.Offer)
								.Include(r => r.Offer.Car)
								.ToListAsync()).Where(r => r.Id == rh.Id && r.Platform == rent.Platform);

						if (temprents.Count() == 1)
						{
							RentHistoryModel? rh2 = temprents.FirstOrDefault();
							if (rh2 != null)
							{
								if (rh.File != null)
									await UploadImage(rh.File);

								rh2.IsReturned = true;
								rh2.Offer.Car.IsRented = false;

								await carContext.SaveChangesAsync();
							}
						}


						ReturnRequestTheir rr = new() {EmployeeEmail = rh.Email, ReturnDescription = "", Base64EncodedCarImage = "" };

						var response = await _client2.PutAsJsonAsync(_client2.BaseAddress + "/Rental/rentals/acceptReturn/" + rh.Id, rr);
						response.EnsureSuccessStatusCode();
						string data = await response.Content.ReadAsStringAsync();

						
					}
					catch (Exception e)
					{
						string x = e.Message;
					}
			}


			return Redirect("/CarApi/AllRents");
		}


		public async Task<bool> UploadImage(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return false;
			}

			// Przesyłanie pliku do Azure Blob Storage
			var fileUrl = await _blobStorageService.UploadFileAsync(file);

			return true;
		}


	}
}
