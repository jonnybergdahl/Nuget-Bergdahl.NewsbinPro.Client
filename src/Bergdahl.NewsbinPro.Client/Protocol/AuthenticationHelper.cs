using System;
using System.Security.Cryptography;
using System.Text;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Builds the authentication string Newsbin expects in the <c>AUTH</c> command.
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Returns the upper case hex MD5 hash of <paramref name="password"/> followed by <paramref name="passwordSalt"/>.
    /// </summary>
    /// <param name="password">The remote control password set in Newsbin.</param>
    /// <param name="passwordSalt">The salt sent by Newsbin when the connection is opened.</param>
    public static string GetAuthString(string password, string passwordSalt)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));
        if (passwordSalt is null)
            throw new ArgumentNullException(nameof(passwordSalt));

        byte[] bytes = Encoding.UTF8.GetBytes(password + passwordSalt);
        byte[] hash;
        using (MD5 md5 = MD5.Create())
        {
            hash = md5.ComputeHash(bytes);
        }

        var builder = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
            builder.Append(b.ToString("X2"));
        return builder.ToString();
    }
}
