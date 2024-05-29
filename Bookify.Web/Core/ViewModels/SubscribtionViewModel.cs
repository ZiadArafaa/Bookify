using Bookify.Web.Core.Consts;

namespace Bookify.Web.Core.ViewModels
{
    public class SubscribtionViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status
        {
            get
            {
                return EndDate < DateTime.Today ? SubscribtionStatus.Expired : StartDate > DateTime.Now ? string.Empty : SubscribtionStatus.Active;
            }
        }
    }
}
