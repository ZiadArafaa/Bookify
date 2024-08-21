using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.Utilities;
using Bookify.Web.Core.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenHtmlToPdf;
using System.IO;
using ViewToHTML.Services;

namespace Bookify.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IViewRendererService _viewRendererService;

        public ReportsController(ApplicationDbContext context, IMapper mapper, IViewRendererService viewRendererService)
        {
            _context = context;
            _mapper = mapper;
            _viewRendererService = viewRendererService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region Books
        public async Task<IActionResult> Books(int? PageIndex, IList<int> SelectedCategories, IList<int> SelectedAuthors)
        {
            BookReportViewModel report = new();

            report.Categories = await _context.Set<Category>()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                .ToListAsync();

            report.Authors = await _context.Set<Author>()
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() })
                .ToListAsync();

            IQueryable<Book> source = _context.Set<Book>()
                .IgnoreQueryFilters()
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .Where(b => (!SelectedAuthors.Any() || SelectedAuthors.Contains(b.AuthorId)) &&
                (!SelectedCategories.Any() || b.Categories.Any(c => SelectedCategories.Contains(c.CategoryId))));

            if (PageIndex is not null && PageIndex.Value > 0)
                report.Books = new PaginatedList<Book>(source, PageIndex.Value, (int)ReportsConfigurations.PageSize);

            return View(report);
        }
        public async Task<IActionResult> ExportBooksReportExcel(IList<int> SelectedCategories, IList<int> SelectedAuthors)
        {
            var books = await _context.Set<Book>()
                .IgnoreQueryFilters()
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .Where(b => (!SelectedAuthors.Any() || SelectedAuthors.Contains(b.AuthorId)) &&
                (!SelectedCategories.Any() || b.Categories.Any(c => SelectedCategories.Contains(c.CategoryId)))).ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Books Report");
            sheet.Cell(1, 1).AddToNamed("FirstCellOfHeader").Value = "Title";
            sheet.Cell(1, 2).Value = "Author";
            sheet.Cell(1, 3).Value = "Created On";
            sheet.Cell(1, 4).Value = "Categories";
            sheet.Cell(1, 5).Value = "Publishing Date";
            sheet.Cell(1, 6).Value = "Hall";
            sheet.Cell(1, 7).Value = "Is Available For Rental?";
            sheet.Cell(1, 8).AddToNamed("LastCellOfHeader").Value = "Status?";

            for (int row = 0; row < books.Count; row++)
            {
                sheet.Cell(row + 2, 1).Value = books[row].Title;
                sheet.Cell(row + 2, 2).Value = books[row].Author!.Name;
                sheet.Cell(row + 2, 3).Value = books[row].CreateOn.ToString("dd ,MMM yyyy");
                sheet.Cell(row + 2, 4).Value = string.Join(" & ", books[row].Categories.Select(c => c.Category!.Name).ToList());
                sheet.Cell(row + 2, 5).Value = books[row].PublishingDate.ToString("dd ,MMM yyyy");
                sheet.Cell(row + 2, 6).Value = books[row].Hall;
                sheet.Cell(row + 2, 7).Value = books[row].IsAvailableForRental ? "yes" : "No";
                sheet.Cell(row + 2, 8).Value = books[row].IsDeleted ? "Not Available" : "Available";
            }

            sheet.FirstRowUsed()
                .CellsUsed()
                .Style.Fill.SetBackgroundColor(XLColor.Black)
                .Font.SetFontColor(XLColor.White);

            sheet.ColumnsUsed().AdjustToContents();

            sheet.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Style.Border.OutsideBorderColor = XLColor.Blue;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"Books_Report_{DateTime.Now.Date}.xlsx");
        }
        public async Task<IActionResult> ExportBooksReportPDF(IList<int> SelectedCategories, IList<int> SelectedAuthors)
        {
            var books = await _context.Set<Book>()
                .IgnoreQueryFilters()
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .Where(b => (!SelectedAuthors.Any() || SelectedAuthors.Contains(b.AuthorId)) &&
                (!SelectedCategories.Any() || b.Categories.Any(c => SelectedCategories.Contains(c.CategoryId)))).ToListAsync();

            var templatePath = "~/Views/Reports/PDFBookTemplete.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, books);

            var pdf = Pdf.From(html).Content();

            return File(pdf, "application/octet-stream", $"Books_Report_{DateTime.Now.Date}.pdf");
        }
        #endregion
        #region Rentals
        public IActionResult Rentals(string Duration, int? PageIndex)
        {
            RentalReportViewModel model = new() { Duration = Duration };

            if (!string.IsNullOrEmpty(Duration))
            {
                var dateRange = Duration.Split(" - ");

                bool isParsedDateFrom = DateTime.TryParse(dateRange[0], out DateTime dateFrom);
                bool isParsedDateTo = DateTime.TryParse(dateRange[1], out DateTime dateTo);

                if (!isParsedDateFrom || !isParsedDateTo)
                {
                    ModelState.AddModelError(nameof(model.Duration), "Date not Valid");
                    return View(model);
                }

                IQueryable<RentalCopy> source = _context.Set<RentalCopy>()
                    .IgnoreQueryFilters()
                    .Where(r => r.RentalDate >= dateFrom && r.RentalDate <= dateTo)
                    .Include(r => r.BookCopy)
                    .ThenInclude(b => b!.Book)
                    .ThenInclude(b => b!.Author)
                    .Include(r => r.Rental)
                    .ThenInclude(r => r!.Subscriber)
                    .OrderBy(r => r.RentalDate);

                if (PageIndex.HasValue)
                    model.Rentals = new PaginatedList<RentalCopy>(source, PageIndex.Value, (int)ReportsConfigurations.PageSize);
            }

            ModelState.Clear();

            return View(model);
        }
        public async Task<IActionResult> ExportRentalsReportExcel(string? Duration)
        {
            if (Duration is null)
                return NotFound();

            var dateRange = Duration.Split(" - ");
            bool isParsedDateFrom = DateTime.TryParse(dateRange[0], out DateTime dateFrom);
            bool isParsedDateTo = DateTime.TryParse(dateRange[1], out DateTime dateTo);

            if (!isParsedDateFrom || !isParsedDateTo)
            {
                RentalReportViewModel model = new();
                ModelState.AddModelError(nameof(model.Duration), "Date not Valid");
                return View(model);
            }

            var rentals = await _context.Set<RentalCopy>()
                .IgnoreQueryFilters()
                .Where(r => r.RentalDate >= dateFrom && r.RentalDate <= dateTo)
                .Include(r => r.BookCopy)
                .ThenInclude(b => b!.Book)
                .ThenInclude(b => b!.Author)
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .OrderBy(r => r.RentalDate).ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Rentals Report");
            sheet.Cell(1, 1).AddToNamed("FirstCellOfHeader").Value = "Subscriber Id";
            sheet.Cell(1, 2).Value = "Subscriber Name";
            sheet.Cell(1, 3).Value = "Subscriber MobileNumber";
            sheet.Cell(1, 4).Value = "Book Title";
            sheet.Cell(1, 5).Value = "Book Author";
            sheet.Cell(1, 6).Value = "Rental Date";
            sheet.Cell(1, 7).Value = "End Date";
            sheet.Cell(1, 8).Value = "Return Date";
            sheet.Cell(1, 9).AddToNamed("LastCellOfHeader").Value = "Extended On";

            for (int row = 0; row < rentals.Count; row++)
            {
                sheet.Cell(row + 2, 1).Value = rentals[row].Rental!.Subscriber!.Id;
                sheet.Cell(row + 2, 2).Value = rentals[row].Rental!.Subscriber!.FirstName + " " + rentals[row].Rental!.Subscriber!.LastName;
                sheet.Cell(row + 2, 3).Value = rentals[row].Rental!.Subscriber!.MobileNumber;
                sheet.Cell(row + 2, 4).Value = rentals[row].BookCopy!.Book!.Title;
                sheet.Cell(row + 2, 5).Value = rentals[row].BookCopy!.Book!.Author!.Name;
                sheet.Cell(row + 2, 6).Value = rentals[row].RentalDate.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 7).Value = rentals[row].EndDate.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 8).Value = rentals[row].ReturnDate?.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 9).Value = rentals[row].ExtendedOn?.ToString("d MMM,yyyy");
            }

            sheet.FirstRowUsed()
                .CellsUsed()
                .Style.Fill.SetBackgroundColor(XLColor.Black)
                .Font.SetFontColor(XLColor.White);

            sheet.ColumnsUsed().AdjustToContents();

            sheet.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Style.Border.OutsideBorderColor = XLColor.Blue;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"Rentals_Report_{DateTime.Now.Date}.xlsx");
        }
        public async Task<IActionResult> ExportRentalsReportPDF(string? Duration)
        {
            if (Duration is null)
                return NotFound();

            var dateRange = Duration.Split(" - ");
            bool isParsedDateFrom = DateTime.TryParse(dateRange[0], out DateTime dateFrom);
            bool isParsedDateTo = DateTime.TryParse(dateRange[1], out DateTime dateTo);

            if (!isParsedDateFrom || !isParsedDateTo)
            {
                RentalReportViewModel model = new();
                ModelState.AddModelError(nameof(model.Duration), "Date not Valid");
                return View(model);
            }

            var rentals = await _context.Set<RentalCopy>()
                .IgnoreQueryFilters()
                .Where(r => r.RentalDate >= dateFrom && r.RentalDate <= dateTo)
                .Include(r => r.BookCopy)
                .ThenInclude(b => b!.Book)
                .ThenInclude(b => b!.Author)
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .OrderBy(r => r.RentalDate).ToListAsync();

            var templatePath = "~/Views/Reports/PDFRentalTemplete.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, rentals);

            var pdf = Pdf.From(html).Content();

            return File(pdf, "application/octet-stream", $"Rentals_Report_{DateTime.Now.Date}.pdf");
        }
        #endregion
        #region DelayedRentals
        public IActionResult DelayedRentals(int? PageIndex)
        {
            IQueryable<RentalCopy> source = _context.Set<RentalCopy>()
                .IgnoreQueryFilters()
                .Include(r => r.BookCopy)
                .ThenInclude(b => b!.Book)
                .ThenInclude(b => b!.Author)
                .Include(r => r.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(r => (!r.ReturnDate.HasValue && DateTime.Now.Date > r.EndDate.Date)
                || (r.ReturnDate.HasValue && r.ReturnDate.Value.Date > r.EndDate.Date))
                .OrderBy(r => r.EndDate);

            var Rentals = new PaginatedList<RentalCopy>(source, PageIndex.HasValue ? PageIndex.Value : 1, (int)ReportsConfigurations.PageSize);

            return View(Rentals);
        }
        public async Task<IActionResult> ExportDelayedRentalsReportExcel()
        {
            var rentals = await _context.Set<RentalCopy>()
               .IgnoreQueryFilters()
               .Include(r => r.BookCopy)
               .ThenInclude(b => b!.Book)
               .ThenInclude(b => b!.Author)
               .Include(r => r.Rental)
               .ThenInclude(r => r!.Subscriber)
               .Where(r => (!r.ReturnDate.HasValue && DateTime.Now.Date > r.EndDate.Date)
               || (r.ReturnDate.HasValue && r.ReturnDate.Value.Date > r.EndDate.Date))
               .OrderBy(r => r.EndDate).ToListAsync();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Rentals Report");
            sheet.Cell(1, 1).AddToNamed("FirstCellOfHeader").Value = "Subscriber Id";
            sheet.Cell(1, 2).Value = "Subscriber Name";
            sheet.Cell(1, 3).Value = "Subscriber MobileNumber";
            sheet.Cell(1, 4).Value = "Book Title";
            sheet.Cell(1, 5).Value = "Book Serial";
            sheet.Cell(1, 6).Value = "Rental Date";
            sheet.Cell(1, 7).Value = "End Date";
            sheet.Cell(1, 8).Value = "Return Date";
            sheet.Cell(1, 9).Value = "Delay In Day(s)";
            sheet.Cell(1, 10).AddToNamed("LastCellOfHeader").Value = "Extended On";

            for (int row = 0; row < rentals.Count; row++)
            {
                sheet.Cell(row + 2, 1).Value = rentals[row].Rental!.Subscriber!.Id;
                sheet.Cell(row + 2, 2).Value = rentals[row].Rental!.Subscriber!.FirstName + " " + rentals[row].Rental!.Subscriber!.LastName;
                sheet.Cell(row + 2, 3).Value = rentals[row].Rental!.Subscriber!.MobileNumber;
                sheet.Cell(row + 2, 4).Value = rentals[row].BookCopy!.Book!.Title;
                sheet.Cell(row + 2, 5).Value = rentals[row].BookCopy!.SerialNumber;
                sheet.Cell(row + 2, 6).Value = rentals[row].RentalDate.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 7).Value = rentals[row].EndDate.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 8).Value = rentals[row].ReturnDate?.ToString("d MMM,yyyy");
                sheet.Cell(row + 2, 9).Value = rentals[row].ReturnDate.HasValue ? (rentals[row].ReturnDate - rentals[row].EndDate)!.Value.Days
                                    : (DateTime.Now - rentals[row].EndDate).Days;
                sheet.Cell(row + 2, 10).Value = rentals[row].ExtendedOn?.ToString("d MMM,yyyy");
            }

            sheet.FirstRowUsed()
                .CellsUsed()
                .Style.Fill.SetBackgroundColor(XLColor.Black)
                .Font.SetFontColor(XLColor.White);

            sheet.ColumnsUsed().AdjustToContents();

            sheet.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Style.Border.OutsideBorderColor = XLColor.Blue;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), "application/octet-stream", $"Delayed_Rentals_Report_{DateTime.Now.Date}.xlsx");
        }
        public async Task<IActionResult> ExportDelayedRentalsReportPDF()
        {
            var rentals = await _context.Set<RentalCopy>()
               .IgnoreQueryFilters()
               .Include(r => r.BookCopy)
               .ThenInclude(b => b!.Book)
               .ThenInclude(b => b!.Author)
               .Include(r => r.Rental)
               .ThenInclude(r => r!.Subscriber)
               .Where(r => (!r.ReturnDate.HasValue && DateTime.Now.Date > r.EndDate.Date)
               || (r.ReturnDate.HasValue && r.ReturnDate.Value.Date > r.EndDate.Date))
               .OrderBy(r => r.EndDate).ToListAsync();

            var templatePath = "~/Views/Reports/PDFDelayedRentalTemplete.cshtml";

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, rentals);

            var pdf = Pdf.From(html).Content();

            return File(pdf, "application/octet-stream", $"Rentals_Report_{DateTime.Now.Date}.pdf");
        }
        #endregion

    }
}
