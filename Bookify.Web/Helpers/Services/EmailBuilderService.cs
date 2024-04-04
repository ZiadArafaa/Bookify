using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;

namespace Bookify.Web.Helpers.Services
{
    public class EmailBuilderService: IEmailBuilderService
    {
        public string GetEmailBody()
        {
           

            //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return $"";
        }
    }
}
