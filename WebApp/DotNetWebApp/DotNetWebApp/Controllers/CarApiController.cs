using DotNetWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotNetWebApp.Controllers
{
	
	public class CarApiController : Controller
	{
		private Uri baseAddress;
		private readonly HttpClient _client;
		private List<Car> _cars;

		public CarApiController(string Uri = "https://localhost:7127/Car")
		{
			baseAddress = new Uri(Uri);
			_client = new HttpClient();
			_client.BaseAddress = baseAddress;
			_cars = new();
		}

        [HttpGet]
		public async Task<IActionResult> Index()
		{
			List<Car> cars = new List<Car>();
			HttpResponseMessage? response = null ;
			try
			{
				response = await _client.GetAsync(baseAddress + "/Get");
			}
			catch(HttpRequestException)
			{
                return View(_cars);
            }

            if (response!= null && response.IsSuccessStatusCode)
			{
				string data = await response.Content.ReadAsStringAsync();
				cars = JsonConvert.DeserializeObject<List<Car>>(data);
                _cars = cars;

            }
			return View(cars);
		}
		
		
	}
}
