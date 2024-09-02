using AutoMapper;
using Bookify.Web.Core.Settings;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Helpers.Services;
using Bookify.Web.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long _imageSize = 2_097_152;
        private IList<string> _imageExtensions = new List<string> { ".jpg", ".png", ".jpeg" };
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IImageService _imageService;


        public BooksController(ICategoryService categoryService, IAuthorService authorService, IBookService bookService
            , IMapper mapper, IWebHostEnvironment webHostEnvironment, IOptions<CloudinarySettings> CloudinaryOptions, ApplicationDbContext applicationDbContext, IImageService imageService)
        {
            _authorService = authorService;
            _categoryService = categoryService;
            _bookService = bookService;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _cloudinarySettings = CloudinaryOptions.Value;
            Account account = new(_cloudinarySettings.Cloud, _cloudinarySettings.ApiKey, _cloudinarySettings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _applicationDbContext = applicationDbContext;
            _imageService = imageService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var book = (await _bookService.GetBooksAsync()).SingleOrDefault(b => b.Id == id);

            if (book is null)
                return NotFound();

;            return View(_mapper.Map<BookViewModel>(book));
        }
        [HttpPost,IgnoreAntiforgeryToken]
        [AjaxOnly]
        public async Task<IActionResult> GetBooks()
        {
            var draw = Request.Form["draw"];
            var start = Request.Form["start"];
            var length = Request.Form["length"];

            var colName = Request.Form[$"columns[{int.Parse(Request.Form["order[0][column]"])}][name]"];
            var orderMethod = Request.Form["order[0][dir]"];

            var serchValue = Request.Form["search[value]"];

            var data = await _applicationDbContext.Set<Book>().Include(b => b.Author)
                .Where(b => b.Title.Contains(serchValue) || b.Author!.Name.Contains(serchValue))
                .OrderBy($"{colName} {orderMethod}")
                .Skip(int.Parse(start)).Take(int.Parse(length)).ToListAsync();
            
            return Ok(new
            {
                draw = draw,
                data = _mapper.Map<IEnumerable<BookViewModel>>(data),
                recordsTotal = _bookService.GetBooksCount(),
                recordsFiltered = _bookService.GetBooksCount()
            });
        }
        public async Task<IActionResult> Create()
        {
            return View("Form", await PopulateBookForm());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateBookForm(model));

            var books = await _bookService.GetBooksAsync();

            var author = await _authorService.IsExistAsync(model.AuthorId);

            if (author is null || author.IsDeleted || books.SingleOrDefault(b => b.AuthorId == model.AuthorId && b.Title == model.Title) is not null)
            {
                ModelState.AddModelError(nameof(model.AuthorId), "Not valid author.");
                return View("Form", await PopulateBookForm(model));
            }

            foreach (var category in model.SelectedCategories)
            {
                if (await _categoryService.IsExistAsync(category) is null)
                {
                    ModelState.AddModelError(nameof(model.SelectedCategories), "Not valid Categories");
                    return View("Form", await PopulateBookForm(model));
                }
            }

            if (model.Image is not null)
            {
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                var path = "images/books";

                var imageResult = await _imageService
                    .UploadAsync(model.Image, path, imageName, hasThumb: true);

                if (!imageResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), imageResult.errorMessage!);
                    return View("Form", await PopulateBookForm(model));
                }
                model.ImageUrl = $"/{path}/{imageName}";
                model.ImageThumbnailUrl = $"/{path}/thumb/{imageName}";

                //using var imageStream = model.Image.OpenReadStream();
                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(imageName, imageStream),
                //    UseFilename = true,
                //    UniqueFilename = false,
                //    Overwrite = true
                //};
                //var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                //model.ImageUrl = uploadResult.SecureUrl.ToString();
                //model.PublicId = uploadResult.PublicId;
                //model.ImageThumbnailUrl = GetBookThumbnailUrl(model.ImageUrl);
            }

            var book = _mapper.Map<Book>(model);
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            var result = await _bookService.AddBookAsync(book);
            if (result < 1)
                return BadRequest("Somthing Error");

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        public async Task<IActionResult> Edit(int id)
        {
            var books = await _bookService.GetBooksAsync();

            var book = books.SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);

            model.SelectedCategories = new List<int>();

            foreach (var category in book.Categories)
            {
                model.SelectedCategories.Add(category.CategoryId);
            }

            return View("Form", await PopulateBookForm(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", await PopulateBookForm(model));

            var books = await _bookService.GetBooksAsync();

            var book = books.SingleOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            var author = await _authorService.IsExistAsync(model.AuthorId);
            if (author is null || author.IsDeleted || books.SingleOrDefault(b => b.AuthorId == model.AuthorId && b.Title == model.Title && b.Id != model.Id) is not null)
            {
                ModelState.AddModelError(nameof(model.AuthorId), "Dublicated Author.");
                return View("Form", await PopulateBookForm(model));
            }

            foreach (var categoryId in model.SelectedCategories)
            {
                var category = await _categoryService.IsExistAsync(categoryId);
                if (category is null || category.IsDeleted)
                {
                    ModelState.AddModelError(nameof(model.SelectedCategories), "Not valid Categories");
                    return View("Form", await PopulateBookForm(model));
                }
            }

            if (model.Image is not null)
            {
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                var path = "images/books";

                var imageResult = await _imageService
                    .UploadAsync(model.Image, path, imageName, hasThumb: true);

                if (!imageResult.isUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), imageResult.errorMessage!);
                    return View("Form", await PopulateBookForm(model));
                }
                model.ImageUrl = $"/{path}/{imageName}";
                if (book.ImageUrl is not null)
                {
                    _imageService.Delete(book.ImageUrl);
                }

                model.ImageThumbnailUrl = $"/{path}/thumb/{imageName}";
                if (book.ImageThumbnailUrl is not null)
                {
                    _imageService.Delete(book.ImageThumbnailUrl);
                }

                //using var imageStream = model.Image.OpenReadStream();
                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(imageName, imageStream),
                //    UseFilename = true,
                //    UniqueFilename = false,
                //    Overwrite = true
                //};
                //var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                //model.ImageUrl = uploadResult.SecureUrl.ToString();
                //model.PublicId = uploadResult.PublicId;
                //model.ImageThumbnailUrl = GetBookThumbnailUrl(model.ImageUrl);

                //if (book.ImageUrl is not null)
                //{
                //    var deletionParams = new DeletionParams(book.PublicId);
                //    await _cloudinary.DestroyAsync(deletionParams);
                //}


            }
            else
            {
                model.ImageUrl = book.ImageUrl;
                model.PublicId = book.PublicId;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }
            //https://res.cloudinary.com/kappo/image/upload/v1706390345/7197ea33-94c4-49dc-899f-b7198661084d.jpg
            //https://res.cloudinary.com/kappo/image/upload/c_thumb,w_200,g_face/v1706390345/7197ea33-94c4-49dc-899f-b7198661084d.jpg

            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;

            book.Categories.Clear();
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category, BookId = model.Id });
            }

            if (!book.IsAvailableForRental)
                foreach (var copy in book.Copies)
                    copy.IsAvailableForRental = false;

            var result = _bookService.UpdateBookAsync(book);

            if (result < 1)
                return BadRequest("Somthing Error");

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var book = await _applicationDbContext.Set<Book>().AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;

            _applicationDbContext.Update(book);
            _applicationDbContext.SaveChanges();

            return Ok(DateTime.Now.ToString());
        }
        public async Task<JsonResult> AllowItem(BookFormViewModel model)
        {
            var book = (await _bookService.GetBooksAsync())
                .SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);

            var isValid = book is null || book.Id == model.Id;

            return Json(isValid);
        }
        private string GetBookThumbnailUrl(string imageUrl)
        {
            return imageUrl.Replace("image/upload/", "image/upload/c_thumb,w_200,g_face/");
        }
        private async Task<BookFormViewModel> PopulateBookForm(BookFormViewModel? bookFormViewModel = null)
        {
            bookFormViewModel ??= new BookFormViewModel();

            bookFormViewModel.Categories = (await _categoryService.GetCategoriesAsync()).Where(c => !c.IsDeleted).OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }).ToList();

            bookFormViewModel.Authors = (await _authorService.GetAuthorsAsync()).Where(c => !c.IsDeleted).OrderBy(c => c.Name)
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() }).ToList();

            return bookFormViewModel;
        }
    }
}
