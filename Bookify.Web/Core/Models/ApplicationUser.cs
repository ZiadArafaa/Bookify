using Microsoft.AspNetCore.Identity;

namespace Bookify.Web.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreateOn { get; set; }
        public string? CreatedById { get; set; } 
        public DateTime? LastUpdatedOn { get; set; }
        public string? UpdatedById { get; set; } 
    }
}
