using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotNetApiCars
{
    public class CarContext : DbContext
    {
        public CarContext(DbContextOptions<CarContext> op) : base(op) { }
        public DbSet<Car> Cars { get; set; } = default!;



    }
}
