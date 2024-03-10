namespace Bookify.Web.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public bool BookIsRental { get; set; }
        [Display(Name = "Is Available For Rental?")]
        public bool IsAvailableForRental { get; set; }
        [Display(Name = "Edittion Number")]
        [Range(1,1000,ErrorMessage ="{0} must size between {1} and {2}")]
        public int EdittionNumber { get; set; }
    }
}
