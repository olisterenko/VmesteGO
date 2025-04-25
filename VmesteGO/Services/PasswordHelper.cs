using System.Security.Cryptography;
using System.Text;

namespace VmesteGO.Services;

public static class PasswordHelper
{
    public static (string PasswordHash, string Salt) HashPassword(string password)
    {
        var salt = Guid.NewGuid().ToString();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (Convert.ToBase64String(hash), salt);
    }

    public static bool VerifyPassword(string storedHash, string storedSalt, string password)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(storedSalt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return Convert.ToBase64String(hash) == storedHash;
    }
}