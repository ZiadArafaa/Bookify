namespace Bookify.Web.Core.Models
{
    public class BaseModel
    {
        public bool IsDeleted { get; set; }
        [MaxLength(450)]
        public string? CreatedById { get; set; } 
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreateOn { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedOn { get; set; }
        [MaxLength(450)]
        public string? UpdatedById { get; set; } 
        public ApplicationUser? UpdatedBy { get; set; }
    }
}
