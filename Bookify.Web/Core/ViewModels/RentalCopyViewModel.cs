namespace Bookify.Web.Core.ViewModels
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
        public int DelayInDays
        {
            get
            {
                return
                    !ReturnDate.HasValue ?
                    DateTime.Now > EndDate ? (int)(DateTime.Now - EndDate).TotalDays : 0
                   : (int)(ReturnDate - EndDate).Value.TotalDays > 0 ? (int)(ReturnDate - EndDate).Value.TotalDays : 0;
            }
        }
    }
}
