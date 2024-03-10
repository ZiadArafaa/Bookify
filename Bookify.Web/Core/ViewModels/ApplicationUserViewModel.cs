namespace Bookify.Web.Core.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreateOn { get; set; }
        public string? CreatedBy { get; set; } 
        public DateTime? LastUpdatedOn { get; set; }
        public string? UpdatedBy { get; set; } 
    }
}
