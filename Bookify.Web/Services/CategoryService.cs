
using Bookify.Web.Core.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        public CategoryService(ApplicationDbContext contex)
        {
            _context = contex;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync() =>
            await _context.Set<Category>().FromSqlRaw("Select * From Library.Categories").AsNoTracking().ToListAsync();

        public async Task<int> CreateCategoryAsync(Category category)
        {
            await _context.AddAsync(category);
            return _context.SaveChanges();
        }
        public int UpdateCategory(Category category)
        {
            _context.Update(category);
            return _context.SaveChanges();
        }

        public async Task<Category?> IsExistAsync(string Name)
        {
            var queryValue = new SqlParameter(nameof(Name), Name);

            var category = await _context.Set<Category>()
                .FromSqlRaw($"EXECUTE dbo.sp_GetCategoryByName @Name ",queryValue).ToListAsync();

            return category.SingleOrDefault();
        }
        public async Task<Category?> IsExistAsync(int Id)
        {
            var queryValue = new SqlParameter(nameof(Id), Id);

            var category = await _context.Set<Category>()
                .FromSqlRaw("EXECUTE dbo.sp_GetCategoryById @Id ", queryValue).ToListAsync();

            return category.SingleOrDefault();
        }
    }
}
