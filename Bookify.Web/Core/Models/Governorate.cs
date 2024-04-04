namespace Bookify.Web.Core.Models
{
    public class Governorate : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Area>? Areas { get; set; }
    }
}
