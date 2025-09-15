using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRentalManagementSystem.Attributes
{
    public class CustomNicAttribute : ValidationAttribute
    {


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var nic = value as string;

            if (string.IsNullOrWhiteSpace(nic))
                return new ValidationResult("NIC is required.");

            nic = nic.Trim().ToUpper();

            // Rajini DA NIC  9 digits + 'V' or 'X'
            var oldNicPattern = @"^\d{9}[VX]$";

            // Vijay Da NIC  12 digits
            var newNicPattern = @"^\d{12}$";

            if (Regex.IsMatch(nic, oldNicPattern) || Regex.IsMatch(nic, newNicPattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid NIC format. Example: 123456789V or 200012345678.");
        }

    }
}