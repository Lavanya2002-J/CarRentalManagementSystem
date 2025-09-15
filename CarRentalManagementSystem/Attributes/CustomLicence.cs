using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRentalManagementSystem.Attributes
{
    public class CustomLicence : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var license = value as string;

            if (string.IsNullOrWhiteSpace(license))
                return new ValidationResult("Driving license number is required.");

            license = license.Trim().ToUpper();

            //   B1234567
            var oldFormatPattern = @"^[A-Z]{1}\d{7}$";

            // 200212345678
            var newFormatPattern = @"^\d{12}$";

            if (Regex.IsMatch(license, oldFormatPattern) || Regex.IsMatch(license, newFormatPattern))
                return ValidationResult.Success;

            return new ValidationResult("Invalid license number format. Valid examples: B1234567 or 200212345678.");
        }

    }
}
