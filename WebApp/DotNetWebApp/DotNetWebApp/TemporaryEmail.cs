using Microsoft.AspNetCore.Identity;

namespace DotNetWebApp
{
	public class EmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			//nic nie robimy a trzeba bedzie chyba
			return Task.CompletedTask; 
		}
	}
}
