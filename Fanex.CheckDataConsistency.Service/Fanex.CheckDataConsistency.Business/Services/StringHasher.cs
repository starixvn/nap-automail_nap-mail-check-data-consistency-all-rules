using System.Security.Cryptography;
using System.Text;

namespace Fanex.CheckDataConsistency.Business.Services;

public static class StringHasher
{
    public static string HashLongStringToSmallString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "null";

        var md5Hash = MD5.Create();
        var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        var builder = new StringBuilder();
        foreach (var t in data)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }
}