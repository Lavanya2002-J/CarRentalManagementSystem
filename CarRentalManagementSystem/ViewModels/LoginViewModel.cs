using CarRentalManagementSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Email or Usernsme")]
        [Required(ErrorMessage = "Username or Email is required.")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [CustomPassword]
        public string Password { get; set; }
    }
}
