using DotNetWebApp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace DotNetApiCars
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("CarContext") ?? throw new InvalidOperationException("Connection string 'CarContext' not found.");
            
            builder.Services.AddDbContext<CarContext>(options =>
               options.UseSqlServer(connectionString));

            // Email Settings
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7136",
                              "https://carrentalwebapp-a5fxc7cne5cmfve8.canadacentral-01.azurewebsites.net",
							  "https://webapp1-ehg7h6g3e9hpaqda.polandcentral-01.azurewebsites.net")

						  .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
            });

            var app = builder.Build();


            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            


            app.UseCors(MyAllowSpecificOrigins);


            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/hello", () => new { message = "Hello, world!" });

            app.Run();
        }
    }
}
