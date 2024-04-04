namespace Bookify.Web.Core.Models
{
    public class Area : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
    }
}
