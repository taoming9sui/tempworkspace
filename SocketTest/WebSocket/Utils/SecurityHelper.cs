using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket.Utils
{
    public class SecurityHelper
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string CreateGuid()
        {
            System.Guid guid = System.Guid.NewGuid();
            return guid.ToString();
        }

        public static string CreateRandomString(int size, char[] charset = null)
        {
            if (charset == null)
                charset = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = charset[Convert.ToInt32(Math.Floor(charset.Length * random.NextDouble()))];
                builder.Append(ch);
            }
            return builder.ToString();
        }  

    }
}
