using Bookify.Web.Core.Models;

namespace Bookify.Web.Services
{
    public interface ICategoryService
    {
        public Task<IEnumerable<Category>> GetCategoriesAsync();
        public Task<int> CreateCategoryAsync(Category category);
        public int UpdateCategory(Category category);
        public Task<Category?> IsExistAsync(string categoryName);
        public Task<Category?> IsExistAsync(int Id);
    }
}
