using DotNetWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DotNetWebApp.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterConfirmationModel> _logger;

        public RegisterConfirmationModel(UserManager<CustomUser> userManager, IEmailSender emailSender, ILogger<RegisterConfirmationModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }
        public string StatusMessage { get; set; }

        // Metoda OnGet służy do wyświetlania strony
        public void OnGet(string email)
        {
            Email = email;
            var user = _userManager.FindByEmailAsync(Email).Result;
            if (user != null)
            {
                // Sprawdzamy status potwierdzenia e-maila
                IsEmailConfirmed = _userManager.IsEmailConfirmedAsync(user).Result;
            }
        }

        // Metoda OnPostAsync obsługuje logikę ponownego wysyłania e-maila
        public async Task<IActionResult> OnPostAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            // Wysyłanie nowego e-maila z potwierdzeniem
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // Dodanie komunikatu informującego o wysłanym e-mailu
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");

            // Przekierowanie do tej samej strony, aby odświeżyć stronę z nowym komunikatem
            return RedirectToPage("/Account/RegisterConfirmation", new { email = email });
        }
    }
}
