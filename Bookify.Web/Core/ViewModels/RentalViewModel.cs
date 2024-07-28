namespace Bookify.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }
        public SubscriberViewModel? Subscriber { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime CreateOn { get; set; }
        public bool HasPenalty { get; set; }
        public ICollection<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();
        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(c => c.DelayInDays);
            }
        }
        public int NumberCopies
        {
            get
            {
                return RentalCopies.Count();
            }
        }
    }
}
