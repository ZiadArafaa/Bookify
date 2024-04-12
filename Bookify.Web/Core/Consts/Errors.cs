namespace Bookify.Web.Core.Consts
{
    public static class Errors
    {
        public const string RangedLength = "{0} must be ranged between {2} and {1}.";
        public const string MaxLength = "{0} must be lower than {1}.";
        public const string DublicatedValue = "{0} is Exist.";
        public const string PasswordMatch = "The password and confirmation password do not match.";
        public const string UsernameMatch = "The {0} Shouldn't contain special.";
        public const string PasswordRegex = "passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least eight characters long.";
        public const string PhoneNumberNotValid = "Phone number not valid.";
        public const string NotAllowedExtension = "Not allowed extension.";
        public const string SizeMustBeLessThan2MB = "Size must be less than 2 MB.";
        public const string NotValidFormate = "Not valid formate";
        public const string NotAllowDate = "Not allow date.";
        public const string NotAllowedItem = "{0} not allowed.";

    }
}
