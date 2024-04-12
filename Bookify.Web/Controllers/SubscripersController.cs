using AutoMapper;
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Helpers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    public class SubscripersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public SubscripersController(ApplicationDbContext context, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Create()
        {
            return View("Form", await PopulateViewModelAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriperFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var path = "images/users";
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image!.FileName);

            var uploadImageResult = await _imageService.UploadAsync(model.Image, path, imageName, true);

            if (!uploadImageResult.isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), uploadImageResult.errorMessage!);
                return View("Form", await PopulateViewModelAsync(model));
            }

            var subscriper = _mapper.Map<Subscriper>(model);

            subscriper.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            subscriper.ImageUrl = $"/{path}/{imageName}";
            subscriper.ImageThumbnailUrl = $"/{path}/thumb/{imageName}";

            await _context.AddAsync(subscriper);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Set<Subscriper>().FindAsync(id);

            if (model is null)
                return NotFound();

            return View("Form", await PopulateViewModelAsync(_mapper.Map<SubscriperFormViewModel>(model)));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriperFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriper = await _context.Set<Subscriper>().FindAsync(model.Id);

            if (subscriper is null)
                return NotFound();


            if (model.Image is not null)
            {
                var path = "images/users";
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var uploadResult = await _imageService.UploadAsync(model.Image, path, imageName, true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", await PopulateViewModelAsync(model));
                }

                model.ImageUrl = $"/images/users/{imageName}";
                model.ImageThumbnailUrl = $"/images/users/thumb/{imageName}";
            }
            else
            {
                model.ImageThumbnailUrl = subscriper.ImageThumbnailUrl;
                model.ImageUrl = subscriper.ImageUrl;
            }

            subscriper = _mapper.Map(model, subscriper);
            subscriper.LastUpdatedOn = DateTime.Now;
            subscriper.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = subscriper.Id });
        }
        [AjaxOnly]
        public async Task<IActionResult> GetAreas(int GovernrateId)
        {
            var result = await _context.Set<Area>()
                .Where(a => a.GovernorateId == GovernrateId).Select(a => new { a.Id, a.Name })
                .OrderBy(a => a.Name).ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SubscriperSearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            var subscriper = await _context.Set<Subscriper>()
                .Where(s => (s.Email == model.Value || s.MobileNumber == model.Value || s.NationalId == model.Value) && !s.IsDeleted).SingleOrDefaultAsync();

            return PartialView("_SearchResponse", _mapper.Map<SubscriperSearchResponseViewModel>(subscriper));
        }
        public async Task<IActionResult> Details(int id)
        {
            var subscriper = await _context.Set<Subscriper>()
                .Include(s=>s.Governorate).Include(s=>s.Area)
                .SingleOrDefaultAsync(m=>m.Id == id);

            if (subscriper is null)
                return NotFound();

            return View(_mapper.Map<SubscriperViewModel>(subscriper));
        }







        public async Task<JsonResult> AllowEmail(SubscriperFormViewModel model)
        {
            var item = await _context.Set<Subscriper>().Where(s => s.Email == model.Email).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowNationalId(SubscriperFormViewModel model)
        {
            var item = await _context.Set<Subscriper>().Where(s => s.NationalId == model.NationalId).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowName(SubscriperFormViewModel model)
        {
            var item = await _context.Set<Subscriper>().Where(s => s.FirstName == model.FirstName && s.LastName == model.LastName).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowMobileNumber(SubscriperFormViewModel model)
        {
            var item = await _context.Set<Subscriper>().Where(s => s.MobileNumber == model.MobileNumber).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }

        private async Task<SubscriperFormViewModel> PopulateViewModelAsync(SubscriperFormViewModel? model = null)
        {
            model ??= new SubscriperFormViewModel();

            model.Governorates = await _context.Set<Governorate>()
                .Select(g => new SelectListItem { Text = g.Name, Value = g.Id.ToString() }).OrderBy(g => g.Text).ToListAsync();

            if (model.GovernorateId != 0)
            {
                model.Areas = await _context.Set<Area>().Where(a => a.GovernorateId == model.GovernorateId).
                Select(g => new SelectListItem { Text = g.Name, Value = g.Id.ToString() }).OrderBy(g => g.Text).ToListAsync();
            }

            return model;
        }
    }
}
