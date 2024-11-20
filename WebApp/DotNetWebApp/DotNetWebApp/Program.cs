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

            //BazyDAnych
            var connectionString = builder.Configuration.GetConnectionString("LogInConnection") ?? throw new InvalidOperationException("Connection string 'LogInConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
			builder.Services.AddDbContext<CarContext>(options => options.UseInMemoryDatabase("Cars"));

			// Email Settings
			builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();

			//dodanie jako naszej Identity naszego CustomUsera (zamiast IdentityUser)
			builder.Services.AddIdentity<CustomUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
							.AddDefaultTokenProviders()
							.AddEntityFrameworkStores<ApplicationDbContext>();
			builder.Services.AddScoped<IUserClaimsPrincipalFactory<CustomUser>, CustomUserClaimsPrincipalFactory>();

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



			// Add Controllers with Views and Razor Pages
			builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

			// Add error handling based on development or production environment
			if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();


            app.UseRouting();

            
            app.UseAuthentication();
            app.UseAuthorization();

            // Default route setup for controllers and Razor Pages
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=CarApi}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

    }
}
