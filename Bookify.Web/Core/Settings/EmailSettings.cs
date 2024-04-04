namespace Bookify.Web.Core.Settings
{
    public class EmailSettings
    {
        public int Port { get; set; } 
        public string Host { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
