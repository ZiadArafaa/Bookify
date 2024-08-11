using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        public BookService(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<int> AddBookAsync(Book book)
        {
            await _context.AddAsync(book);

            return _context.SaveChanges();
        }
        public int UpdateBookAsync(Book book)
        {
            _context.Update(book);

            return _context.SaveChanges();
        }
        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _context.Set<Book>()
                .Include(b => b.Copies).Include(a => a.Author).Include(c => c.Categories).ThenInclude(c => c.Category).ToListAsync();
        }
        public int GetBooksCount()
        {
            return _context.Set<Book>().Count();
        }
    }
}
