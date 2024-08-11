using AutoMapper;
using Bookify.Web.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public DashboardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var numberOfCopies = await _context.Set<BookCopy>().CountAsync(c => !c.IsDeleted);
            var numberOfMembers = await _context.Set<Subscriber>().CountAsync(b => !b.IsDeleted);


            var lastAddBooks = await _context.Set<Book>().Where(b => !b.IsDeleted).Include(b => b.Author)
                .OrderByDescending(b => b.Id).Take(8).ToListAsync();

            var topBooks = await _context.Set<RentalCopy>()
                .Include(r => r.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(b => b!.Author)
                .GroupBy(c => new
                {
                    c.BookCopy!.BookId,
                    c.BookCopy.Book!.Title,
                    c.BookCopy.Book!.ImageThumbnailUrl,
                    AuthorName = c.BookCopy.Book.Author!.Name
                })
                .OrderByDescending(r => r.Count())
                .Take(6)
                .Select(r => new BookViewModel
                {
                    Id = r.Key.BookId,
                    Title = r.Key.Title,
                    ImageThumbnailUrl = r.Key.ImageThumbnailUrl,
                    Author = r.Key.AuthorName
                }).ToListAsync();

            DashboardViewModel viewModel = new()
            {
                NumberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10,
                NumberOfUsers = numberOfMembers,
                LastAddBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddBooks),
                TopBooks = _mapper.Map<IEnumerable<BookViewModel>>(topBooks),
            };


            return View(viewModel);
        }
        public async Task<IActionResult> RentalPerDays()
        {
            var startDate = DateTime.Today.AddDays(-29).Date;
            var endDate = DateTime.Today;

            var Data = await _context.Set<RentalCopy>()
                .IgnoreQueryFilters()
                .Where(c => c.RentalDate.Date >= startDate && c.RentalDate.Date <= endDate)
                .GroupBy(c => c.RentalDate)
                .Select(c => new RentalPerDayViewModel
                {
                    Key = c.Key.ToString("dd MMM"),
                    Value = c.Count().ToString()
                })
                .ToListAsync();

            List<RentalPerDayViewModel> newData = new();
            for (var countDate = startDate; countDate <= endDate; countDate = countDate.AddDays(1))
            {
                RentalPerDayViewModel record = new();

                record.Key = countDate.ToString("dd MMM");
                record.Value = Data.Where(r => r.Key == record.Key).Select(r => r.Value).SingleOrDefault() ?? "0";

                newData.Add(record);
            }


            return Ok(newData);
        }
        public async Task<IActionResult> SubscriberByCity()
        {
            var subscriberByCity = await _context.Set<Subscriber>()
                .Include(s => s.Governorate)
                .GroupBy(s => s.Governorate!.Name)
                .Select(s => new
                {
                    s.Key,
                    Value = s.Count().ToString(),
                }).ToListAsync();

            return Ok(subscriberByCity);
        }
    }
}
