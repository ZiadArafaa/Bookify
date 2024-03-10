using Bookify.Web.Core.Models;

namespace Bookify.Web.Services
{
    public interface IAuthorService
    {
        public Task<IEnumerable<Author>> GetAuthorsAsync();
        public Task<Author?> IsExistAsync(int id);
        public Task<Author?> IsExistAsync(string name);
        public Task<int> AddAsync(Author author);
        public int Update(Author author);

    }
}
