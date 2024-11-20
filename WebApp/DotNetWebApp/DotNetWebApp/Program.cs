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

            // Add the database context with the connection string
            var connectionString = builder.Configuration.GetConnectionString("LogInConnection") ?? throw new InvalidOperationException("Connection string 'LogInConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Email Settings
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add Identity and token providers (this fixes the missing IUserTwoFactorTokenProvider)
            builder.Services.AddIdentity<CustomUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                // You can configure token options here if necessary
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();  // Register token providers like Email, Password, Two-Factor Auth, etc.

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<CustomUser>, CustomUserClaimsPrincipalFactory>();

            // Add the CarContext with InMemoryDatabase
            builder.Services.AddDbContext<CarContext>(options => options.UseInMemoryDatabase("Cars"));

            // Add Authentication services (Identity, Google Authentication, etc.)
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add Authentication and Authorization
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
