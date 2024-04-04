namespace Bookify.Web.Core.Models
{
    public class Subscriper : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Email { get; set; } = null!;
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;
        [MaxLength(100)]
        public string LastName { get; set; } = null!;
        public DateTime BirthOfDate { get; set; }
        [MaxLength(500)]
        public string Address { get; set; } = null!;
        [MaxLength(20)]
        public string NationalId { get; set; } = null!;
        [MaxLength(20)]
        public string MobileNumber { get; set; } = null!;
        public bool HasWhatsApp { get; set; } 
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }
        public int AreaId { get; set; }
        public Area? Area { get; set; }
        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;
        [MaxLength(500)]
        public string ImageThumbnailUrl { get; set; } = null!;
        public bool IsBlackListed { get; set; }
    }
}
