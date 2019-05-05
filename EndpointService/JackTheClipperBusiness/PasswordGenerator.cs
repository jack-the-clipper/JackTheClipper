using System;
using System.Collections.Generic;
using System.Text;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Generates a new random password.
    /// </summary>
    public class PasswordGenerator
    {
        private static readonly Random rnd = new Random();
        private static readonly int pwLength = 16;
        
        /// <summary>
        /// Generates a random password with 16 characters length.
        /// </summary>
        /// <returns>Password as string.</returns>
        public static string GeneratePw()
        {
            const string pwContent = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz§$%&/()=?!*#-_";
            System.Text.StringBuilder pwString = new StringBuilder();

            for (int i = 0; i < pwLength; i++)
            {
                pwString.Append(pwContent[rnd.Next(pwContent.Length)]);
            }
            return pwString.ToString();
        }
    }
}
