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
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        public SubscribersController(ApplicationDbContext context, IMapper mapper, IImageService imageService)
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
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var path = "images/subscribers";
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image!.FileName);

            var uploadImageResult = await _imageService.UploadAsync(model.Image, path, imageName, true);

            if (!uploadImageResult.isUploaded)
            {
                ModelState.AddModelError(nameof(model.Image), uploadImageResult.errorMessage!);
                return View("Form", await PopulateViewModelAsync(model));
            }

            var subscriper = _mapper.Map<Subscriber>(model);

            subscriper.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            subscriper.ImageUrl = $"/{path}/{imageName}";
            subscriper.ImageThumbnailUrl = $"/{path}/thumb/{imageName}";

            await _context.AddAsync(subscriper);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Set<Subscriber>().FindAsync(id);

            if (model is null)
                return NotFound();

            return View("Form", await PopulateViewModelAsync(_mapper.Map<SubscriberFormViewModel>(model)));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateViewModelAsync(model));

            var subscriper = await _context.Set<Subscriber>().FindAsync(model.Id);

            if (subscriper is null)
                return NotFound();


            if (model.Image is not null)
            {
                var path = "images/subscribers";
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var uploadResult = await _imageService.UploadAsync(model.Image, path, imageName, true);

                if (!uploadResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), uploadResult.errorMessage!);
                    return View("Form", await PopulateViewModelAsync(model));
                }

                 _imageService.Delete(subscriper.ImageUrl);
                 _imageService.Delete(subscriper.ImageThumbnailUrl);

                model.ImageUrl = $"/{path}/{imageName}";
                model.ImageThumbnailUrl = $"/{path}/thumb/{imageName}";
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
        public async Task<IActionResult> Search(SubscribersearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            var subscriper = await _context.Set<Subscriber>()
                .Where(s => (s.Email == model.Value || s.MobileNumber == model.Value || s.NationalId == model.Value) && !s.IsDeleted).SingleOrDefaultAsync();

            return PartialView("_SearchResponse", _mapper.Map<SubscribersearchResponseViewModel>(subscriper));
        }
        public async Task<IActionResult> Details(int id)
        {
            var subscriper = await _context.Set<Subscriber>()
                .Include(s=>s.Governorate).Include(s=>s.Area)
                .SingleOrDefaultAsync(m=>m.Id == id);

            if (subscriper is null)
                return NotFound();

            return View(_mapper.Map<SubscriberViewModel>(subscriper));
        }







        public async Task<JsonResult> AllowEmail(SubscriberFormViewModel model)
        {
            var item = await _context.Set<Subscriber>().Where(s => s.Email == model.Email).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowNationalId(SubscriberFormViewModel model)
        {
            var item = await _context.Set<Subscriber>().Where(s => s.NationalId == model.NationalId).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowName(SubscriberFormViewModel model)
        {
            var item = await _context.Set<Subscriber>().Where(s => s.FirstName == model.FirstName && s.LastName == model.LastName).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }
        public async Task<JsonResult> AllowMobileNumber(SubscriberFormViewModel model)
        {
            var item = await _context.Set<Subscriber>().Where(s => s.MobileNumber == model.MobileNumber).FirstOrDefaultAsync();

            var allowed = item == null || item.Id == model.Id;

            return Json(allowed);
        }

        private async Task<SubscriberFormViewModel> PopulateViewModelAsync(SubscriberFormViewModel? model = null)
        {
            model ??= new SubscriberFormViewModel();

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
