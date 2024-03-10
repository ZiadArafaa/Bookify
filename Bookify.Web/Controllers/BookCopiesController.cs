using AutoMapper;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [AjaxOnly]
        public async Task<IActionResult> Create(int bookId)
        {
            var book = await _context.Set<Book>().AsNoTracking().SingleOrDefaultAsync(b => b.Id == bookId);
            if (book is null)
                return NotFound();

            return PartialView("_Form", new BookCopyFormViewModel() { BookId = book.Id, BookIsRental = book.IsAvailableForRental });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = await _context.Set<Book>().AsNoTracking().SingleOrDefaultAsync(b => b.Id == model.BookId);
            if (book is null)
                return NotFound();

            var copy = _mapper.Map<BookCopy>(model);

            copy.IsAvailableForRental = book.IsAvailableForRental && copy.IsAvailableForRental;

            await _context.AddAsync(copy);
            _context.SaveChanges();

            copy.Book = book;

           return PartialView("_BookCopyRow", _mapper.Map<BookCopyViewModel>(copy));
        }
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var copy = await _context.Set<BookCopy>().Include(b => b.Book).SingleOrDefaultAsync(b => b.Id == id);

            if (copy == null)
                return NotFound();

            var model = _mapper.Map<BookCopyFormViewModel>(copy);

            model.BookIsRental = copy.Book!.IsAvailableForRental;

            return PartialView("_Form", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = await _context.Set<BookCopy>().FindAsync(model.Id);
            var book = await _context.Set<Book>().FindAsync(model.BookId);

            if (copy is null || book is null)
                return NotFound();

            copy = _mapper.Map(model, copy);

            copy.IsAvailableForRental = copy.IsAvailableForRental && book.IsAvailableForRental;

            _context.Update(copy);
            _context.SaveChanges();

            return PartialView("_BookCopyRow", _mapper.Map<BookCopyViewModel>(copy));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var copy = await _context.Set<BookCopy>().AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;

            _context.Update(copy);
            _context.SaveChanges();

            return Ok();
        }
    }
}
