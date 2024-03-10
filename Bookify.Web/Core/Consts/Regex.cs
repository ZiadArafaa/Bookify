namespace Bookify.Web.Core.Consts;
public struct Regex {
    public const string Password = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
    public const string Username = "^[a-zA-Z0-9]+$";
}
