using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }
        [StringLength(100, MinimumLength = 4, ErrorMessage = Errors.RangedLength), Display(Name = "Author")]
        [Remote("ValidateDublicated", "Authors", AdditionalFields = "Id", ErrorMessage = "{0} is Exist.")]
        public string Name { get; set; } = null!;
    }
}
