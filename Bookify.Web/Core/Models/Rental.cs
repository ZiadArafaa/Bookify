namespace Bookify.Web.Core.Models
{
    [Table(name: "Rentals", Schema = "Library")]
    public class Rental : BaseModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber? Subscriber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public bool HasPenalty { get; set; }
        public ICollection<RentalCopy> RentalCopies { get; set; } = new List<RentalCopy>();
    }
}
