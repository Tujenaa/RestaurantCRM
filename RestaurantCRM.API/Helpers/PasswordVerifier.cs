using System.Security.Cryptography;
using System.Text;

namespace RestaurantCRM.API.Helpers;

public static class PasswordVerifier
{
    public static bool Verify(string password, string? storedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedPassword))
        {
            return false;
        }

        if (storedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }

        // SeedData stores MD5 hashes. Keep these accounts usable until they are
        // migrated to BCrypt, and retain support for older plain-text records.
        var md5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
        if (storedPassword.Length == 32 && storedPassword.All(Uri.IsHexDigit))
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(md5), Encoding.ASCII.GetBytes(storedPassword.ToLowerInvariant()));
        }

        return string.Equals(password, storedPassword, StringComparison.Ordinal);
    }
}
