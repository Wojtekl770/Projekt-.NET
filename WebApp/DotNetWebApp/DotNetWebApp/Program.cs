using DotNetWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetWebApp.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DotNetWebApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			//dodanie bazy danych z connectionstringa
			var connectionString = builder.Configuration.GetConnectionString("LogInConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Email Settings
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();

			//dodanie jako naszej Identity naszego CustomUsera (zamiast IdentityUser)
			builder.Services.AddIdentity<CustomUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
							.AddDefaultTokenProviders()
							.AddEntityFrameworkStores<ApplicationDbContext>();
			builder.Services.AddScoped<IUserClaimsPrincipalFactory<CustomUser>, CustomUserClaimsPrincipalFactory>();

			//dodanie naszego glownego contextu CarApi
			builder.Services.AddDbContext<CarContext>(options=> options.UseInMemoryDatabase("Cars"));

			//dodanie autentykacji defaultowej (IdentityUser) i tej z google
			builder.Services.AddAuthentication(o =>
							 {
								 o.DefaultScheme = IdentityConstants.ApplicationScheme;
								 o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
							 })
							 .AddCookie()
							 .AddGoogle(options =>
							 {
								 options.ClientId = "881753906009-bridd3jbnb6o9s11v53chnlcl7shi7ck.apps.googleusercontent.com";
								 options.ClientSecret = "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";
							 });

			//Dodanie Controlerow z widokami i stron
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages();



			var app = builder.Build();

			//dodanie errorow w zaleznosci od tego czy development czy production
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			//dodanie logowania
			app.UseAuthentication();
			app.UseAuthorization();


			//defaultowo znajdujemy sie na tej stronie
			app.MapControllerRoute(
				name: "default",
				//pattern: "{controller=Home}/{action=Index}/{id?}");
				pattern: "{controller=CarApi}/{action=Index}/{id?}");
			app.MapRazorPages();



			app.Run();
		}
	}
}
