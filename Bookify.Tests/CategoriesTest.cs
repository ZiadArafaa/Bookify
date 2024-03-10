using AutoMapper;
using Bookify.Web.Controllers;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Text;

namespace Bookify.Tests
{
    public class CategoriesTest
    {
        [Fact]
        public async void Create_WhenModelNameIsExist_ReturnBadRequest()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();

            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))!
                .Returns(Task.FromResult(new Category { Id = new Random(1).Next() }));

            A.CallTo(() => categoryService.CreateCategoryAsync(A<Category>.Ignored))!
                .Returns(Task.FromResult(1));


            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Create(new CategoryFormViewModel());

            Assert.Equivalent(new BadRequestResult(), result);
        }
        [Fact]
        public async void Create_WhenModelNameIsValid_ReturnRowItem()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();

            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))
                .Returns(Task.FromResult(null as Category));
            A.CallTo(() => categoryService.CreateCategoryAsync(A<Category>.Ignored))
                .Returns(Task.FromResult(1));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Create(new CategoryFormViewModel()) as PartialViewResult;

            Assert.Equivalent("_CategoryRow", result?.ViewName);
        }
        [Fact]
        public async void GetEditView_WhenIdIsNotExist_ReturnNotFound()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
                .Returns(Task.FromResult(null as Category));


            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Edit(new Random().Next());

            Assert.Equivalent(new NotFoundResult(), result);
        }
        [Fact]
        public async void GetEditView_WhenIdIsExist_ReturnForm()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
                .Returns(Task.FromResult(new Category()));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = (await testObject.Edit(new Random(1).Next())) as PartialViewResult;

            Assert.Equivalent("_Form", result?.ViewName);
        }
        [Fact]
        public async void Edit_WhenIdIsNotExist_ReturnNotFound()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();
            A.CallTo(() => categoryService.UpdateCategory(A<Category>.Ignored))
                .Returns(1);
            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))
                .Returns(Task.FromResult(null as Category));

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
                .Returns(Task.FromResult(null as Category));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Edit(new CategoryFormViewModel());

            Assert.Equivalent(new NotFoundResult(), result);
        }
        [Fact]
        public async void Edit_WhenNameIsExistWithDifferntId_ReturnBadRequest()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();

            A.CallTo(() => categoryService.UpdateCategory(A<Category>.Ignored)).Returns(1);

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
               .Returns(Task.FromResult(new Category()));

            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))!
            .Returns(Task.FromResult(new Category() { Id = new Random(1).Next() }));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Edit(new CategoryFormViewModel());

            Assert.Equivalent(new BadRequestResult(), result);
        }
        [Fact]
        public async void Edit_WhenNameIsExistWithSameId_ReturnCategoryRow()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();
            var sameId = new Random(1).Next();

            A.CallTo(() => categoryService.UpdateCategory(A<Category>.Ignored)).Returns(1);

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
               .Returns(Task.FromResult(new Category()));

            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))!
               .Returns(Task.FromResult(new Category() { Id = sameId }));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Edit(new CategoryFormViewModel() { Id=sameId}) as PartialViewResult;

            Assert.Equivalent("_CategoryRow", result?.ViewName);
        }
        [Fact]
        public async void Edit_WhenNameIsNotExist_ReturnCategoryRow()
        {
            var categoryService = A.Fake<ICategoryService>();
            var mapper = A.Fake<IMapper>();
            var sameId = new Random(1).Next();

            A.CallTo(() => categoryService.UpdateCategory(A<Category>.Ignored)).Returns(1);

            A.CallTo(() => categoryService.IsExistAsync(A<int>.Ignored))!
               .Returns(Task.FromResult(new Category()));

            A.CallTo(() => categoryService.IsExistAsync(A<string>.Ignored))!
               .Returns(Task.FromResult(null as Category));

            var testObject = new CategoriesController(categoryService, mapper);

            var result = await testObject.Edit(new CategoryFormViewModel()) as PartialViewResult;

            Assert.Equivalent("_CategoryRow", result?.ViewName);
        }

    }
}
