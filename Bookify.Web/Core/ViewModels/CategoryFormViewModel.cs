using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [StringLength(100, MinimumLength = 4, ErrorMessage = Errors.RangedLength), Display(Name = "Category")]
        [Remote("ValidateDublicated", "Categories", AdditionalFields = "Id", ErrorMessage = Errors.DublicatedValue)]
        public string Name { get; set; } = null!;
    }
}