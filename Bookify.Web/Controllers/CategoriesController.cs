using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.Validations;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(_mapper.Map<IEnumerable<CategoryViewModel>>(await _categoryService.GetCategoriesAsync()));
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            return PartialView("_Form");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid || !bool.Parse((await ValidateDublicated(model)).Value!.ToString()!))
                return BadRequest();

            var category = _mapper.Map<Category>(model);

            if ((await _categoryService.CreateCategoryAsync(category)).Equals(0))
                return BadRequest();

            return PartialView("_CategoryRow", _mapper.Map<CategoryViewModel>(category));
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.IsExistAsync(id);

            if (model is null)
                return NotFound();

            return PartialView("_Form", _mapper.Map<CategoryFormViewModel>(model));
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = await _categoryService.IsExistAsync(model.Id);

            if (category is null)
                return NotFound();

            if (!bool.Parse((await ValidateDublicated(model)).Value!.ToString()!))
                return BadRequest();

            category = _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;

            if (_categoryService.UpdateCategory(category).Equals(0))
                return BadRequest();


            return PartialView("_CategoryRow",_mapper.Map<CategoryViewModel>(category));
        }
        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {

            var category = await _categoryService.IsExistAsync(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;

            if (_categoryService.UpdateCategory(category).Equals(0))
                return BadRequest();

            return Ok(category.LastUpdatedOn.ToString());
        }


        public async Task<JsonResult> ValidateDublicated(CategoryFormViewModel model)
        {
            var category = await _categoryService.IsExistAsync(model.Name);

            bool IsValid = category is null || category.Id == model.Id;

            return Json(IsValid);
        }
    }

}
