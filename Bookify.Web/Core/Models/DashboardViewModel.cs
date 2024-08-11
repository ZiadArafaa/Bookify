using Bookify.Web.Core.ViewModels;

namespace Bookify.Web.Core.Models
{
    public class DashboardViewModel
    {
        public int NumberOfCopies { get; set; } 
        public int NumberOfUsers { get; set; } 
        public IEnumerable<BookViewModel> LastAddBooks { get; set; } = new List<BookViewModel>();
        public IEnumerable<BookViewModel> TopBooks { get; set; } = new List<BookViewModel>();
    }
}
