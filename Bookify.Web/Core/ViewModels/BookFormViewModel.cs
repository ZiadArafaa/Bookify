using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }
        [StringLength(100, ErrorMessage = Errors.RangedLength)]
        [Remote("AllowItem", null!, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DublicatedValue)]
        public string Title { get; set; } = null!;
        [StringLength(100, ErrorMessage = Errors.RangedLength)]
        public string Publisher { get; set; } = null!;
        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = Errors.NotAllowDate)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? PublicId { get; set; }
        [StringLength(50, ErrorMessage = Errors.RangedLength)]
        public string Hall { get; set; } = null!;
        [Display(Name = "Is available for rental ?")]
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;
        [Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.DublicatedValue)]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }
        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = null!;
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
