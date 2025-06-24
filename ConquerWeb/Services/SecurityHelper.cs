using System;
using System.Security.Cryptography;

namespace ConquerWeb.Services
{
    public class SecurityHelper
    {
        public bool VerifyPassword(string plaintextPassword, string storedPassword)
        {
            return plaintextPassword == storedPassword;
        }

        public string HashPassword(string plaintextPassword)
        {
            return plaintextPassword;
        }

        public string GenerateResetToken()
        {
            byte[] randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber)
                        .Replace("+", "-")
                        .Replace("/", "_")
                        .Replace("=", "");
        }
    }
}