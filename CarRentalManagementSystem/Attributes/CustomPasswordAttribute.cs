using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRentalManagementSystem.Attributes
{
    public class CustomPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult("Password is required.");

            if (password.Length < 8)
                return new ValidationResult("Password must be at least 8 characters long.");

            if (password.Contains(" "))
                return new ValidationResult("Password cannot contain spaces.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return new ValidationResult("Password must contain at least one uppercase letter.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return new ValidationResult("Password must contain at least one lowercase letter.");

            if (!Regex.IsMatch(password, @"\d"))
                return new ValidationResult("Password must contain at least one number.");

            if (!Regex.IsMatch(password, @"[@$!%*?&]"))
                return new ValidationResult("Password must contain at least one special character (@, $, !, %, *, ?, &).");

            return ValidationResult.Success;
        }
    }
}
