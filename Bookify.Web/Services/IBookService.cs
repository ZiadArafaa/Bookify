namespace Bookify.Web.Services
{
    public interface IBookService
    {
        public Task<int> AddBookAsync(Book book);
        public int UpdateBookAsync(Book book);
        public Task<IEnumerable<Book>> GetBooksAsync();
        public int GetBooksCount();
    }
}
