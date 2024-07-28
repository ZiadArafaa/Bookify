using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }
        [AssertThat("(TotalDlayInDays != 0 && PenaltyPaid == true) || TotalDlayInDays == 0",ErrorMessage = "Penalty should be paid.")]
        [Display(Name = "Penalty Paid?")]
        public bool PenaltyPaid { get; set; }
        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();
        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new();
        public bool AllowedForExtension { get; set; }
        public int TotalDlayInDays
        {
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}
