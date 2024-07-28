using AutoMapper;
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Helpers.Services;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IDataProtector _dataProtector;
        private readonly IEmailSender _emailSender;
        public SubscribersController(ApplicationDbContext context,
            IMapper mapper, IImageService imageService, IDataProtectionProvider dataProtector, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _dataProtector = dataProtector.CreateProtector("SecureSubscriber");
            _emailSender = emailSender;
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

            var subscribtion = new Subscribtion
            {
                CreatedById = subscriper.CreatedById,
                CreatedOn = subscriper.CreateOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
            };

            subscriper.Subscribtions.Add(subscribtion);

            await _context.AddAsync(subscriper);
            _context.SaveChanges();

            BackgroundJob.Enqueue(() => _emailSender
               .SendEmailAsync(subscriper.Email, "Bookify Subscribtion", "Created Processing Successfully Enjoy The Adventure Now 😍 !"));


            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(string id)
        {
            int subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var model = await _context.Set<Subscriber>().FindAsync(subscriberId);

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

            return RedirectToAction(nameof(Index));
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

            if (subscriper is null)
                return PartialView("_SearchResponse", null);

            var viewModel = _mapper.Map<SubscribersearchResponseViewModel>(subscriper);
            viewModel.Id = _dataProtector.Protect(viewModel.Id);

            return PartialView("_SearchResponse", viewModel);
        }
        public async Task<IActionResult> Details(string id)
        {
            int subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriper = await _context.Set<Subscriber>()
                .Include(s => s.Governorate).Include(s => s.Area)
                .Include(s => s.Subscribtions)
                .Include(s => s.Rentals.OrderByDescending(r => r.CreateOn))
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefaultAsync(m => m.Id == subscriberId);

            if (subscriper is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriper);
            viewModel.Id = _dataProtector.Protect(viewModel.Id);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> Renew(string sKey)
        {
            int subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = await _context.Set<Subscriber>().Include(s => s.Subscribtions).SingleOrDefaultAsync(s => s.Id == subscriberId);

            if (subscriber is null) return NotFound();
            if (subscriber.IsBlackListed || subscriber.IsDeleted) return BadRequest();

            var lastSubscrbtion = subscriber.Subscribtions.Last();
            var startDate = lastSubscrbtion.EndDate < DateTime.Today ? DateTime.Today : lastSubscrbtion.EndDate.AddDays(1);

            Subscribtion subscribtion = new()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Today,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            subscriber.Subscribtions.Add(subscribtion);
            _context.SaveChanges();

            BackgroundJob.Enqueue(() => _emailSender.
                SendEmailAsync(subscriber.Email, "Bookify Renewal", $"<p>Renew Process Succeded Enjoy Now 😍 !<p/>" +
                $"will expired at : {subscribtion.EndDate.ToString("dd MMM,yyyy")}"));

            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> MarkRentalDeleted(int id)
        {
            var rental = await _context.Set<Rental>().Include(r => r.RentalCopies).SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null || rental.CreateOn.Date != DateTime.Today.Date)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Today;
            rental.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();

            return Ok(new { copiesCount = rental.RentalCopies.Count });
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
