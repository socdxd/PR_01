using System;
using System.Security.Cryptography;
using System.Text;

namespace WPF_Payment_Project.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            
            string cleanPhone = phone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("+", "");
            return cleanPhone.Length >= 10 && cleanPhone.Length <= 11 && long.TryParse(cleanPhone, out _);
        }

        public static bool IsStrongPassword(string password)
        {
            if (password.Length < 6)
                return false;

            bool hasUpper = false, hasLower = false, hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                if (char.IsLower(c)) hasLower = true;
                if (char.IsDigit(c)) hasDigit = true;
            }
            
            return hasUpper && hasLower && hasDigit;
        }
    }
}
