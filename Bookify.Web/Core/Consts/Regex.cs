namespace Bookify.Web.Core.Consts;
public struct Regex {
    public const string Password = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
    public const string Username = "^[a-zA-Z0-9]+$";
    public const string PhoneNnumber = "^01[1025]{1}[0-9]{8}";
    public const string NationalId = "^[23]{1}[0-9]{13}";
}
