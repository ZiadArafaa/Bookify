namespace Bookify.Web.Core.Models
{
    public class Category : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<BookCategory>? Books { get; set; }
    }
}
