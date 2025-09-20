using CarRentalManagementSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        [Required, DataType(DataType.Password)]
        [CustomPassword]
        public string NewPassword { get; set; }
        [Required, DataType(DataType.Password), Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
