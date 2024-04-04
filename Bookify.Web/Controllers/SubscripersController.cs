using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
    public class SubscripersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscripersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {

            SubscriperFormViewModel model = new()
            {
                Governorates = _context.Set<Governorate>()
                .Select(g => new SelectListItem { Text = g.Name, Value = g.Id.ToString() }).OrderBy(g => g.Text).ToList()
            };

            return View("Form", model);
        }
    }
}
