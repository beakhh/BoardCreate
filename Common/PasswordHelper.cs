using System;
using System.Security.Cryptography;
using System.Text;

namespace BoardCreate.Common
{
    public class PasswordHelper
    {
        public static string GenerateSalt(int size = 16)
        {
            byte[] saltBytes = new byte[size];
            RandomNumberGenerator.Fill(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        public static string GenerateSaltedHash(string UserPW, string salt)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(salt + UserPW));
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

}
