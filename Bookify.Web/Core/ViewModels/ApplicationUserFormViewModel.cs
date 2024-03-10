using Bookify.Web.Core.Consts;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class ApplicationUserFormViewModel
    {
        public string? Id { get; set; }
        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = Errors.RangedLength, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;
        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = Errors.RangedLength, MinimumLength = 2)]
        public string LastName { get; set; } = null!;
        [StringLength(100, ErrorMessage = Errors.RangedLength, MinimumLength = 2)]
        [Display(Name = "Username")]
        [Remote("AllowUsername", "Users", AdditionalFields = "Id", ErrorMessage = Errors.DublicatedValue)]
        [RegularExpression(Regex.Username,ErrorMessage =Errors.UsernameMatch)]
        public string UserName { get; set; } = null!;
        [EmailAddress]
        [Remote("AllowEmail", "Users", AdditionalFields = "Id", ErrorMessage = Errors.DublicatedValue)]
        public string Email { get; set; } = null!;
        [StringLength(30, ErrorMessage = Errors.RangedLength, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(Regex.Password, ErrorMessage = Errors.PasswordRegex)]
        [RequiredIf("Id == null")]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = Errors.PasswordMatch)]
        [RequiredIf("Id == null")]
        public string? ConfirmPassword { get; set; }
        public IEnumerable<SelectListItem>? Roles { get; set; }
        public ICollection<string> SelectedRoles { get; set; } = null!;
    }
}
