using Bookify.Web.Core.Consts;

namespace Bookify.Web.Core.ViewModels
{
    public class ResetFormViewModel
    {
        public string Id { get; set; } = null!;
        [DataType(DataType.Password)]
        [RegularExpression(Regex.Password,ErrorMessage =Errors.PasswordRegex)]
        public string Password { get; set; } = null!;
        [Compare("Password",ErrorMessage =Errors.PasswordMatch)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
