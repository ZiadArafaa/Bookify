using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorService authorService, IMapper mapper)
        {
            _authorService = authorService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            return View(_mapper.Map<IEnumerable<AuthorViewModel>>(await _authorService.GetAuthorsAsync()));
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!bool.Parse((await ValidateDublicated(model)).Value!.ToString()!))
                return BadRequest();

            var author = _mapper.Map<Author>(model);

            if ((await _authorService.AddAsync(author)).Equals(0))
                return BadRequest();

            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(author));
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var author = await _authorService.IsExistAsync(id);

            if (author is null)
                return NotFound();

            return PartialView("_Form", _mapper.Map<AuthorFormViewModel>(author));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = await _authorService.IsExistAsync(model.Id);

            if (author is null)
                return NotFound();

            if (!bool.Parse((await ValidateDublicated(model)).Value!.ToString()!))
                return BadRequest();

            author = _mapper.Map(model, author);
            author.LastUpdatedOn = DateTime.Now;

            if (_authorService.Update(author).Equals(0))
                return BadRequest();

            return PartialView("_AuthorRow",_mapper.Map<AuthorViewModel>(author));
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var author = await _authorService.IsExistAsync(id);
            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;

            if (_authorService.Update(author).Equals(0))
                return BadRequest();

            return Ok(author.LastUpdatedOn.ToString());
        }
        public async Task<JsonResult> ValidateDublicated(AuthorFormViewModel model)
        {
            var author = await _authorService.IsExistAsync(model.Name);
            bool IsValid = author is null || author.Id == model.Id;

            return Json(IsValid);
        }
    }
}
