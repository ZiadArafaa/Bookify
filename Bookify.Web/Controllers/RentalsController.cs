using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public RentalsController(IMapper mapper, ApplicationDbContext context, IDataProtectionProvider provider)
        {
            _mapper = mapper;
            _context = context;
            _dataProtector = provider.CreateProtector("SecureSubscriber");
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Create(string sKey)
        {
            var (ErrorMessage, MaxCopiesForRentals) = await ValidateCreateRental(sKey);

            if (!string.IsNullOrEmpty(ErrorMessage))
                return View("StopRental", ErrorMessage);

            return View("Form", new RentalFormViewModel { SubscriberKey = sKey, MaxRentalsAllowed = MaxCopiesForRentals });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ErrorMessage, copies, subscriber) = await ValidateCopyAsync(model);

            if (!string.IsNullOrEmpty(ErrorMessage))
                return View("StopRental", ErrorMessage);

            var rental = new Rental
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                RentalCopies = copies!
            };

            subscriber!.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        public async Task<IActionResult> Edit(int id)
        {
            var rental = await _context.Set<Rental>()
                .Select(r => new { CreateOn = r.CreateOn, SubscriberId = r.SubscriberId, Id = r.Id })
                .SingleOrDefaultAsync(r => r.Id == id);

            if (rental is null || rental.CreateOn.Date != DateTime.Now.Date)
                return NotFound();

            var (ErrorMessage, MaxCopiesForRentals) = await ValidateCreateRental(_dataProtector.Protect(rental.SubscriberId.ToString()), rental.Id);

            if (!string.IsNullOrEmpty(ErrorMessage))
                return View("StopRental", ErrorMessage);

            var CurrentCopies = _mapper.Map<List<BookCopyViewModel>>(await _context.Set<Rental>().Where(r => r.Id == rental.Id)
                .Include(r => r.RentalCopies)
                .ThenInclude(b => b.BookCopy)
                .ThenInclude(b => b!.Book)
                .SelectMany(r => r.RentalCopies)
                .Where(r => !r.ReturnDate.HasValue)
                .Select(r => r.BookCopy).ToListAsync());

            return View("Form", new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
                MaxRentalsAllowed = MaxCopiesForRentals,
                Id = rental.Id,
                CurrentCopies = CurrentCopies
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var (ErrorMessage, copies, subscriber) = await ValidateCopyAsync(model);

            if (!string.IsNullOrEmpty(ErrorMessage))
                return View("StopRental", ErrorMessage);

            var rental = await _context.Set<Rental>()
               .Include(r => r.RentalCopies)
               .SingleOrDefaultAsync(r => r.Id == model.Id && r.CreateOn.Date == DateTime.Now.Date);

            if (rental is null)
                return NotFound();

            rental.LastUpdatedOn = DateTime.Now;
            rental.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            rental.RentalCopies = copies!;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        public async Task<IActionResult> Return(int id)
        {
            var rental = await _context.Set<Rental>()
                .Where(r => r.Id == id && r.CreateOn.Date != DateTime.Now.Date && r.RentalCopies.Any(c => !c.ReturnDate.HasValue))
                .Include(s => s.Subscriber)
                .ThenInclude(s => s!.Subscribtions)
                .Include(r => r.RentalCopies)
                .ThenInclude(r => r.BookCopy)
                .ThenInclude(r => r!.Book)
                .SingleOrDefaultAsync();

            if (rental is null)
                return NotFound();

            RentalReturnFormViewModel viewModel;
            PopulateReturnFormViewModel(out viewModel, in rental);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(RentalReturnFormViewModel model)
        {
            var rental = await _context.Set<Rental>()
               .Where(r => r.Id == model.Id && r.CreateOn.Date != DateTime.Now.Date && r.RentalCopies.Any(c => !c.ReturnDate.HasValue))
               .Include(s => s.Subscriber)
               .ThenInclude(s => s!.Subscribtions)
               .Include(r => r.RentalCopies)
               .ThenInclude(r => r.BookCopy)
               .ThenInclude(r => r!.Book)
               .SingleOrDefaultAsync();

            if (rental is null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateReturnFormViewModel(out model, in rental);
                ModelState.AddModelError("", "Model Not Valid");
                return View(model);
            }

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                var error = string.Empty;

                if (rental.Subscriber!.IsBlackListed)
                    error = "Subscriper is a black listed";
                else if (DateTime.Now.Date > rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration).Date)
                    error = $"not allowed to extend after {(int)RentalConfiguration.RentalDuration} day(s)";
                else if (rental.RentalCopies.Any(c => !c.ReturnDate.HasValue && DateTime.Now.Date > c.EndDate.Date))
                    error = "return copy on time and try to extend again";
                else if (rental.Subscriber.Subscribtions.Last().EndDate.Date < rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration * 2).Date)
                    error = "renew subscription and try again";

                if (!string.IsNullOrEmpty(error))
                {
                    PopulateReturnFormViewModel(out model, rental);
                    ModelState.AddModelError("", error);

                    return View(model);
                }
            }

            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
            {
                if (!copy.IsReturned.HasValue)
                    continue;

                var currentCopy = rental.RentalCopies.FirstOrDefault(c => c.BookCopyId == copy.Id);

                if (currentCopy is null)
                    continue;

                if (copy.IsReturned.Value)
                {
                    if (currentCopy.ReturnDate.HasValue)
                        continue;

                    currentCopy.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }
                else if (!copy.IsReturned.Value)
                {
                    if (currentCopy.ExtendedOn.HasValue)
                        continue;

                    currentCopy.ExtendedOn = DateTime.Now;
                    currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalConfiguration.RentalDuration * 2);
                    isUpdated = true;
                }

            }

            if (isUpdated)
            {
                rental.LastUpdatedOn = DateTime.Now;
                rental.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                rental.HasPenalty = model.PenaltyPaid;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { Id = rental.Id });
        }
        [HttpPost]
        [AjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(RentalSearchViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", ModelState);

            var copy = await _context.Set<BookCopy>().Include(b => b.Book)
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.SerialNumber.ToString() == model.Value && !b.IsDeleted && !b.Book!.IsDeleted);

            if (copy is null)
                return NotFound("copy Not Found");

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest("copy/book not available ");

            var isRentaled = _context.Set<RentalCopy>().Any(r => r.BookCopyId == copy.Id && !r.ReturnDate.HasValue);

            if (isRentaled)
                return BadRequest("copy is rentaled already ");

            var copyViewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_SelectedBooks", copyViewModel);
        }
        public async Task<IActionResult> Details(int id)
        {
            var rentals = await _context.Set<Rental>()
                .Include(r => r.RentalCopies)
                .ThenInclude(b => b.BookCopy)
                .ThenInclude(b => b!.Book).SingleOrDefaultAsync(r => r.Id == id);

            if (rentals is null)
                return NotFound();

            return View(_mapper.Map<RentalViewModel>(rentals));
        }
        [HttpPost]
        [AjaxOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRentalCopy(int bookCopyId)
        {
            var copy = await _context.Set<RentalCopy>().SingleOrDefaultAsync(c => c.BookCopyId == bookCopyId && !c.ReturnDate.HasValue);

            if (copy is null)
                return NotFound();

            copy.ReturnDate = DateTime.Now;
            _context.SaveChanges();

            return Ok();
        }
        public async Task<IActionResult> RentalHistory(int id)
        {
            var rentals = await _context.Set<RentalCopy>()
                .IgnoreQueryFilters()
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(r => r.BookCopyId == id).ToListAsync();

            if (rentals.Count() == 0)
                return NotFound();

            return View(_mapper.Map<IEnumerable<RentalHistoryViewModel>>(rentals));
        }

        private async Task<(string ErrorMessage, int? MaxCopiesForRentals)> ValidateCreateRental(string sKey, int? rentalId = null)
        {
            int subscriberId;
            try
            {
                var isParsed = int.TryParse(_dataProtector.Unprotect(sKey), out subscriberId);

                if (!isParsed)
                    return ("Subscriber Not Found", null);
            }
            catch (Exception ex)
            {
                return ("Subscriber Not Found", null);
            }

            var subscriber = await _context.Set<Subscriber>()
                .Include(s => s.Subscribtions).Include(s => s.Rentals).ThenInclude(s => s.RentalCopies)
                .SingleOrDefaultAsync(s => s.Id == subscriberId || !s.IsDeleted);

            if (subscriber is null)
                return ("Subscriber Not Found", null);

            if (subscriber.IsBlackListed)
                return ("Subscriber is black listed", null);

            if (subscriber.Subscribtions.Last().EndDate < DateTime.Now.AddDays((int)RentalConfiguration.RentalDuration))
                return ("Subscribtion Will expire soon ,please renew and try again", null);

            if (subscriber.Rentals.SelectMany(r => r.RentalCopies).Any(r => !r.ReturnDate.HasValue && r.EndDate <= DateTime.Now))
                return ("please return book copy(s) and try again", null);

            var numberOfRentalCopies = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies).Where(c => !c.ReturnDate.HasValue).Count();

            if (numberOfRentalCopies.Equals((int)RentalConfiguration.MaxCopiesForRental))
                return ("please return book copy(s) and try again", null);

            var maxCopiesForRentals = (int)RentalConfiguration.MaxCopiesForRental - numberOfRentalCopies;

            return (string.Empty, maxCopiesForRentals);
        }

        private async Task<(string ErrorMessage, List<RentalCopy>? copies, Subscriber? subscriber)> ValidateCopyAsync(RentalFormViewModel model)
        {
            if (model.SelectedCopies.Count().Equals(0))
                return ("Model State Not Valid", null, null);

            var (ErrorMessage, MaxCopiesForRentals) = await ValidateCreateRental(model.SubscriberKey, model.Id);

            if (!string.IsNullOrEmpty(ErrorMessage))
                return (ErrorMessage, null, null);

            if (model.SelectedCopies.Count() > MaxCopiesForRentals)
                return ($"Max number for rental {MaxCopiesForRentals}", null, null);

            var selectedCopies = model.SelectedCopies.Distinct().ToList();

            var selectedBookCopies = await _context.Set<BookCopy>().Include(c => c.Book).Where(c => selectedCopies.Contains(c.Id)).ToListAsync();

            if (selectedBookCopies.DistinctBy(c => c.BookId).Count() != selectedBookCopies.Count)
                return ("not allowed to rental more than one copy of the same book.", null, null);

            var isValidSelectedBooks = selectedBookCopies.Count == selectedCopies.Count
                && !await _context.Set<RentalCopy>().AnyAsync(c => selectedCopies.Contains(c.BookCopyId) && !c.ReturnDate.HasValue && (!model.Id.HasValue || c.RentalId != model.Id.Value));

            if (!isValidSelectedBooks)
                return ("Copy not available For Rental", null, null);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = await _context.Set<Subscriber>().FindAsync(subscriberId);

            var rentaledBooks = await _context.Set<Rental>()
                .Where(r => r.SubscriberId == subscriberId)
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .SelectMany(r => r.RentalCopies.Where(rc => !rc.ReturnDate.HasValue && (!model.Id.HasValue || rc.RentalId != model.Id.Value)).Select(c => c.BookCopy)
                .Select(b => b!.BookId)).ToListAsync();

            var rentalCopies = new List<RentalCopy>();
            foreach (var selectedBookCopy in selectedBookCopies)
            {
                if (selectedBookCopy.Book!.IsDeleted || !selectedBookCopy.Book.IsAvailableForRental)
                    return ("Book for this copy not available For Rental", null, null);

                if (rentaledBooks.Contains(selectedBookCopy.Book.Id))
                    return ($"cant be rentaled {selectedBookCopy.Book.Title}. you already have copy of it!", null, null);

                rentalCopies.Add(new RentalCopy { BookCopyId = selectedBookCopy.Id });
            }


            return (string.Empty, rentalCopies, subscriber);
        }
        private void PopulateReturnFormViewModel(out RentalReturnFormViewModel model, in Rental rental)
        {
            model = new();

            model.Id = rental.Id;

            model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(r => !r.ReturnDate.HasValue).ToList());

            model.SelectedCopies = rental.RentalCopies.Where(r => !r.ReturnDate.HasValue).Select(r => new ReturnCopyViewModel
            { Id = r.BookCopyId, IsReturned = r.ExtendedOn.HasValue ? false : null }).ToList();

            model.AllowedForExtension = !rental.Subscriber!.IsBlackListed
            && !rental.RentalCopies.Any(c => !c.ReturnDate.HasValue && c.EndDate.Date < DateTime.Now.Date)
            && DateTime.Now.Date <= rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration)
            && rental.Subscriber.Subscribtions.Last().EndDate.Date >= rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration * 2).Date;
        }
    }
}
