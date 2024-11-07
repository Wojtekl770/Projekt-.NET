using DotNetWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApp.Data
{
	
		public class CarContext : DbContext
		{
			public CarContext() { }
			public CarContext(DbContextOptions<CarContext> op) : base(op) { }
			public DbSet<Car> Cars { get; set; }



		}
	
}
