using DotNetWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApp.Data
{

	public class CarContext : DbContext
	{
		public CarContext() { }
		public CarContext(DbContextOptions<CarContext> op) : base(op) { }
		public DbSet<CarPlatform> Cars { get; set; }
		//public DbSet<CarPlusDomain> CarsPlusDomain { get; set; }
		public DbSet<OfferCarModel> Offers {get; set;}
		//public DbSet<RentHistory> Rents { get; set; }
        public DbSet<RentHistoryModel> Rents { get; set; }

    }

}
