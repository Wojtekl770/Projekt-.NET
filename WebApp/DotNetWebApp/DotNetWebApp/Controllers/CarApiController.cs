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
using System.Drawing;
using System.Linq;
//using AspNetCore;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.WebRequestMethods;
using Azure.Storage.Blobs;
using DotNetWebApp.Models;

namespace DotNetWebApp.Controllers
{

    public class CarApiController : Controller
    {
        private string Uri;
        //private Uri baseAddress;
        private readonly HttpClient _client;
        private CarContext carContext;
        private readonly BlobStorageService _blobStorageService;
        public CarApiController(CarContext carC)
        {
            string Uri = "https://localhost:7127/Car";
            //Uri = "https://carrentalapi2-acg4cgdcanecabap.canadacentral-01.azurewebsites.net/Car";

            this.Uri = Uri;
            Uri baseAddress = new Uri(this.Uri);
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
            carContext = carC;
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            _blobStorageService = new();
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
                response = await _client.GetAsync(_client.BaseAddress + "/Get");
            }
            catch (HttpRequestException e)
            {
                response = null;
            }

            if (response != null && response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                cars = JsonConvert.DeserializeObject<List<Car>>(data);
                List<CarPlatform> templist;

                if (cars != null)
                {
                    //jezeli w pamieci podrecznej brak aut
                    if (!carContext.Cars.Any())
                        foreach (Car car in cars)
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
                        foreach (Car car in cars)
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
                                    CarId = car.Id,
                                    Platform = Uri
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
                final.Sort((c1, c2) => c1.CarId.CompareTo(c2.CarId));
                return View(final);
            }
            catch (Exception)
            {
                return Redirect("/CarApi/Index");
            }
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


                var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/Rent", oc);
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
                    CarPlatform car = (await carContext.Cars.ToListAsync()).First(c => offerCarID.CarId == c.Id && c.Platform == offerCarID.Platform);
                    car.IsRented = true;
                    await carContext.SaveChangesAsync();
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

            await Rents(rr, true);

            return View(await carContext.Rents.Include(r => r.Offer).Include(r => r.Offer.Car).ToListAsync());

        }

        [HttpPut]
        private async Task<int> Rents(RentsRequest rr, bool onlyNotReturned)
        {
            try
            {
                if (onlyNotReturned)
                    //ususwanie zwroconych wynajmow - wsm tu sie nic nie dzieje xd
                    foreach (RentHistoryModel rh in (await carContext.Rents.ToListAsync()))
                        if (rh.IsReturned)
                        {
                            //rh.Offer.Car = null;
                            rh.Offer = null;
                            carContext.Remove(rh);
                            await carContext.SaveChangesAsync();
                        }


                //pobranie wynajmow
                var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/GetMyRents", rr);
                response.EnsureSuccessStatusCode();
                string data = await response.Content.ReadAsStringAsync();
                IEnumerable<RentHistory>? rents = JsonConvert.DeserializeObject<IEnumerable<RentHistory>>(data);

                if (rents == null)
                    return -1;
                //return View(await carContext.Rents.ToListAsync());


                if (!carContext.Rents.Any())
                    foreach (RentHistory rent in rents)
                    {
                        if (!onlyNotReturned || !rent.IsReturned)
                        {
                            RentHistoryModel rentModel = new(rent, Uri);

                            carContext.Add(rentModel);
                            await carContext.SaveChangesAsync();


                            //dodanie referencji na auto i oferte
                            rentModel.Offer = (await carContext.Offers.ToListAsync()).FirstOrDefault(o => o.Id == rentModel.OfferId && o.Platform == rentModel.Platform);
                            await carContext.SaveChangesAsync();
                            if (rentModel.Offer == null)
                            {
                                rentModel.Offer = new(rent.Offer, Uri);
                                carContext.Add(rentModel.Offer);
                                // carContext.Update(rentModel);
                                await carContext.SaveChangesAsync();


                                //nie POWINNO byc problemyu z dodaniem erferencji na auto
                                rentModel.Offer.Car = (await carContext.Cars.ToListAsync()).FirstOrDefault(c => c.Id == rentModel.Offer.CarId && c.Platform == rentModel.Platform);
                                await carContext.SaveChangesAsync();
                            }


                        }
                    }
                else
                    foreach (RentHistory rent in rents)
                    {
                        RentHistoryModel? temprent;
                        RentHistoryModel rentModel = new(rent, Uri);


                        //dodanie takich ktorych nie ma w naszej lokalnej DB i nie sa zwrocone (lub chcemy tez nie zwrocone)
                        if ((temprent = (await carContext.Rents.ToListAsync()).Where(r => r.Id == rentModel.Id && r.Platform == rentModel.Platform).FirstOrDefault()) == null && (!onlyNotReturned || !rentModel.IsReturned))
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
                                //carContext.Update(rentModel);
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
                            //jezeli jest w bazie danych ale trzeba usunac ja bo juz zwrocilismy (chyba ze chcemy te zwrocone juz ogladac)
                            if (onlyNotReturned && rentModel.IsReturned)
                            {
                                //if (temprent.Offer != null)
                                //temprent.Offer.Car = null;
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

            ReturnRequest rr = new() { Client_Id = id_client, Email = email, Platform = platform, Rent_Id = rh.Id };

            try
            {
                //proba zwrotu?
                var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/Return", rr);
                response.EnsureSuccessStatusCode();
                string data = await response.Content.ReadAsStringAsync();
                bool? success = JsonConvert.DeserializeObject<bool>(data);

                if (success != null && (bool)success)
                {
                    IEnumerable<RentHistoryModel> temprents = (await carContext.Rents
                        .Include(r => r.Offer)
                        .Include(r => r.Offer.Car)
                        .ToListAsync()).Where(r => r.Id == rh.Id && r.Platform == Uri);

                    if (temprents.Count() == 1)
                    {
                        RentHistoryModel? rh2 = temprents.FirstOrDefault();
                        if (rh2 != null)
                        {
                            rh2.IsReadyToReturn = true;
                            //rh2.IsReturned = true;
                            //rh2.Offer.Car.IsRented = false;

                            //zeny z mojej bazy nie ususnac auta i oferty
                            //rh2.Offer = null;
                            //carContext.Remove(rh2);

                            await carContext.SaveChangesAsync();
                        }
                    }

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

        public async Task<IActionResult> ConfirmReturn(RentHistory rh) //tylko id poprawne tutaj
        {
            string platform = Request.Host.ToString();


            ReturnRequest rr = new() { Client_Id = rh.Client_Id, Email = rh.Email, Platform = platform, Rent_Id = rh.Id };

            if (rh.IsReadyToReturn)
                try
                {
                    //proba zwrotu?
                    var response = await _client.PutAsJsonAsync(_client.BaseAddress + "/ConfirmReturn", rr);
                    response.EnsureSuccessStatusCode();
                    string data = await response.Content.ReadAsStringAsync();
                    bool? success = JsonConvert.DeserializeObject<bool>(data);

                    if (success != null && (bool)success)
                    {
                        IEnumerable<RentHistoryModel> temprents = (await carContext.Rents
                            .Include(r => r.Offer)
                            .Include(r => r.Offer.Car)
                            .ToListAsync()).Where(r => r.Id == rh.Id && r.Platform == Uri);

                        if (temprents.Count() == 1)
                        {
                            RentHistoryModel? rh2 = temprents.FirstOrDefault();
                            if (rh2 != null)
                            {
                                rh2.IsReturned = true;
                                rh2.Offer.Car.IsRented = false;

                                //zeny z mojej bazy nie ususnac auta i oferty
                                //rh2.Offer = null;
                                //carContext.Remove(rh2);

                                //await carContext.SaveChangesAsync();
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    string x = e.Message;
                }


            return Redirect("/CarApi/MyRents");
        }




        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Przesyłanie pliku do Azure Blob Storage
            var fileUrl = await _blobStorageService.UploadFileAsync(file);

            return Ok(new { FileUrl = fileUrl });
        }


    }
}
