using AutoMapper;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public SearchController(IMapper mapper, ApplicationDbContext context, IDataProtectionProvider dataProtector)
        {
            _mapper = mapper;
            _context = context;
            _dataProtector = dataProtector.CreateProtector("ProtectBooks");
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> FindBook(string Key)
        {
            var book = await _context.Set<Book>()
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted && (b.Title.Contains(Key) || b.Author!.Name.Contains(Key)))
                .Select(b => new { b.Title, Author = b.Author!.Name, Key = _dataProtector.Protect(b.Id.ToString()) })
                .ToListAsync();

            return Ok(book);
        }
        public async Task<IActionResult> Details(string Key)
        {
            var id = int.Parse(_dataProtector.Unprotect(Key));

            var book = await _context.Set<Book>()
                .Include(b => b.Author)
                .Include(b=>b.Copies)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return NotFound();

            return View(_mapper.Map<BookViewModel>(book));
        }
    }
}
