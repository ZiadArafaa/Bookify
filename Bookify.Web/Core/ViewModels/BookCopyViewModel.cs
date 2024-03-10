namespace Bookify.Web.Core.ViewModels
{
    public class BookCopyViewModel 
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookHall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public int SerialNumber { get; set; }
        public int EdittionNumber { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateOn { get; set; }
    }
}
