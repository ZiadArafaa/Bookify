namespace Bookify.Web.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => (PageIndex > 1);
        public bool HasNextPage => (PageIndex + 1 <= TotalPages);
        public int Start
        {
            get
            {
                var start = 1;

                if (TotalPages > (int)ReportsConfigurations.MaximumPages)
                {
                    start = PageIndex;

                    if (start != 1)
                        start = PageIndex - 1;

                    if (start + (int)ReportsConfigurations.MaximumPages - 1 >= TotalPages)
                        start = TotalPages - ((int)ReportsConfigurations.MaximumPages - 1);
                }

                return start;
            }
        }
        public int End
        {
            get
            {
                var end = TotalPages;

                if (TotalPages > (int)ReportsConfigurations.MaximumPages)
                {
                    end = Start + (int)ReportsConfigurations.MaximumPages - 1;

                    if (end >= TotalPages)
                        end = TotalPages;
                }

                return end;
            }
        }
    }
}
