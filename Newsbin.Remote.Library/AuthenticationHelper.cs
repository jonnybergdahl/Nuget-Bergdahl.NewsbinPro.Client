using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Newsbin.Remote
{
    public static class AuthenticationHelper
    {
        public static string GetAuthString(string password, string passwordSalt)
        {
            password = password + passwordSalt;
            MD5CryptoServiceProvider md5CryptoProvider = new MD5CryptoServiceProvider();
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(password);
            byteArray = md5CryptoProvider.ComputeHash(byteArray);
            StringBuilder builder = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                builder.AppendFormat("{0:X2}", b);
            }
            return builder.ToString();
        }
    }
}
