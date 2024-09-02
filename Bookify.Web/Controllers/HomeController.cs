using AutoMapper;
using Bookify.Web.Core.Consts;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Bookify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context
            , IMapper mapper, IDataProtectionProvider dataProtector)
        {
            _logger = logger;
            _signInManager = signInManager;
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("ProtectBooks");
        }
        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Dashboard");

            var lastAddBooks = await _context.Set<Book>().Where(b => !b.IsDeleted).Include(b => b.Author)
                .OrderByDescending(b => b.Id).Take(10).ToListAsync();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddBooks);

            foreach (var book in viewModel)
                book.Key = _dataProtector.Protect(book.Id.ToString());

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = 500)
        {
            return View(new ErrorViewModel
            {
                ErrorMessage = ReasonPhrases.GetReasonPhrase(statusCode)
                ,
                StatusCode = statusCode
            });
        }
    }
}
