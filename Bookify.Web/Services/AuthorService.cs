using Bookify.Web.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly ApplicationDbContext _context;
        public AuthorService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Author>> GetAuthorsAsync()
            => await _context.Set<Author>().AsNoTracking().ToListAsync();

        public async Task<Author?> IsExistAsync(int id)
            => await _context.Set<Author>().FindAsync(id);

        public async Task<Author?> IsExistAsync(string name)
            => await _context.Set<Author>().SingleOrDefaultAsync(a => a.Name == name);
        public async Task<int> AddAsync(Author author)
        {
            await _context.AddAsync(author);
            return _context.SaveChanges();
        }
        public int Update(Author author)
        {
            _context.Update(author);
            return _context.SaveChanges();
        }
    }
}

