using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class SubscriperFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "First name")]
        [RegularExpression(Regex.Username, ErrorMessage = Errors.UsernameMatch)]
        public string FirstName { get; set; } = null!;
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Last name")]
        [RegularExpression(Regex.Username, ErrorMessage = Errors.UsernameMatch)]
        public string LastName { get; set; } = null!;
        [Display(Name = "Birth of date")]
        [AssertThat("BirthOfDate < Today() ", ErrorMessage =Errors.NotAllowDate)]
        public DateTime BirthOfDate { get; set; }
        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string Address { get; set; } = null!;
        [MaxLength(14, ErrorMessage = Errors.MaxLength),Display(Name = "National Id")]
        [RegularExpression(Regex.NationalId, ErrorMessage = Errors.NotValidFormate)]
        public string NationalId { get; set; } = null!;
        [MaxLength(20, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(Regex.PhoneNnumber, ErrorMessage = Errors.PhoneNumberNotValid)]
        [Display(Name = "Mobile number")]
        public string MobileNumber { get; set; } = null!;
        [Display(Name = "Has Whatsapp ?")]
        public bool HasWhatsApp { get; set; }
        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem> Governorates { get; set; } = new List<SelectListItem>();
        [Display(Name = "Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public IFormFile Image { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ImageThumbUrl { get; set; }
    }
}