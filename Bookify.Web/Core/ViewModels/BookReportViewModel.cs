using Bookify.Web.Core.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.ViewModels
{
    public class BookReportViewModel
    {
        public IList<SelectListItem>? Authors { get; set; }
        public IList<int> SelectedAuthors { get; set; } = new List<int>();
        public IList<SelectListItem>? Categories { get; set; }
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public PaginatedList<Book>? Books { get; set; }
    }
}
