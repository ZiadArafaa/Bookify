namespace Bookify.Web.Core.ViewModels
{
    public class RentalHistoryViewModel
    {
        public string SubscriperName { get; set; } = null!;
        public string SubscriperMobile { get; set; } = null!;
        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
        public int TotalDelay
        {
            get
            {
                return ReturnDate.HasValue ?
                    ReturnDate.Value < EndDate ? 0 : (int)(ReturnDate.Value - EndDate).TotalDays
                    : EndDate > DateTime.Now ? 0 : (int)(DateTime.Now - EndDate).TotalDays;
            }
        }
    }
}
