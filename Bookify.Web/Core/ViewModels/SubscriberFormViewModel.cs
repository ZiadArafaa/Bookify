using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [EmailAddress]
        [Remote(action: "AllowEmail", controller: "Subscribers", AdditionalFields = "Id", ErrorMessage = Errors.NotAllowedItem)]
        public string Email { get; set; } = null!;
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "First name")]
        [RegularExpression(Regex.Username, ErrorMessage = Errors.UsernameMatch)]
        [Remote(action: "AllowName", controller: "Subscribers", AdditionalFields = "Id,LastName", ErrorMessage = Errors.NotAllowedItem)]
        public string FirstName { get; set; } = null!;
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Last name")]
        [RegularExpression(Regex.Username, ErrorMessage = Errors.UsernameMatch)]
        [Remote(action: "AllowName", controller: "Subscribers", AdditionalFields = "Id,FirstName", ErrorMessage = Errors.NotAllowedItem)]
        public string LastName { get; set; } = null!;
        [Display(Name = "Birth of date")]
        [AssertThat("BirthOfDate < Today() ", ErrorMessage = Errors.NotAllowDate)]
        public DateTime BirthOfDate { get; set; }
        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string Address { get; set; } = null!;
        [MaxLength(14, ErrorMessage = Errors.MaxLength), Display(Name = "National Id")]
        [Remote(action: "AllowNationalId", controller: "Subscribers", AdditionalFields = "Id", ErrorMessage = Errors.NotAllowedItem)]
        [RegularExpression(Regex.NationalId, ErrorMessage = Errors.NotValidFormate)]
        public string NationalId { get; set; } = null!;
        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Mobile number")]
        [RegularExpression(Regex.PhoneNnumber, ErrorMessage = Errors.PhoneNumberNotValid)]
        [Remote(action: "AllowMobileNumber", controller: "Subscribers", AdditionalFields = "Id", ErrorMessage = Errors.NotAllowedItem)]
        public string MobileNumber { get; set; } = null!;
        [Display(Name = "Has Whatsapp ?")]
        public bool HasWhatsApp { get; set; }
        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem> Governorates { get; set; } = new List<SelectListItem>();
        [Display(Name = "Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        [RequiredIf("Id==0", ErrorMessage = "The {0} field is required.")]
        public IFormFile? Image { get; set; } 
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
    }
}