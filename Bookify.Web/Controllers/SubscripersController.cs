using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Create()
        {

            SubscriperFormViewModel model = new()
            {
                Governorates = await _context.Set<Governorate>()
                .Select(g => new SelectListItem { Text = g.Name, Value = g.Id.ToString() }).OrderBy(g => g.Text).ToListAsync()
            };

            return View("Form", model);
        }
        [AjaxOnly]
        public async Task<IActionResult> GetAreas(int GovernrateId)
        {
            var result = await _context.Set<Area>()
                .Where(a => a.GovernorateId == GovernrateId).Select(a => new { a.Id, a.Name })
                .OrderBy(a => a.Name).ToListAsync();

            return Ok(result);
        }
    }
}
