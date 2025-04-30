using System.Text.RegularExpressions;

namespace BankAccount.Writer;

public static class ExtensionMethods
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        const string pattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        return Regex.IsMatch(email, pattern);
    }
}
