using CarRentalManagementSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class ProfileViewModel
    {

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [CustomEmail]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "NIC is required.")]
        [CustomNic]
        public string NIC { get; set; }

        [Required(ErrorMessage = "License Number is required.")]
        [Display(Name = "License Number")]
        [CustomLicence]
        public string LicenseNo { get; set; }

        // --- Password Change Fields ---
        // These are not required; the user only fills them if they want to change the password.

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        [CustomPassword]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password")]
        [CustomPassword]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [CustomPassword]
        public string ConfirmNewPassword { get; set; }
    }
}

