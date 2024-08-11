
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
            await _context.Set<Category>().AsNoTracking().ToListAsync();

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
            return await _context.Set<Category>().SingleOrDefaultAsync(c => c.Name == Name);
        }
        public async Task<Category?> IsExistAsync(int Id)
        {
            return await _context.Set<Category>().FindAsync(Id);
        }
    }
}
