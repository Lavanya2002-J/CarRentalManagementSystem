using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters")]
        public string Password { get; set; }  // Plain text as per specs (no hashing)

        [Required(ErrorMessage = "Role is required")]
        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
        public string Role { get; set; }  // "Admin" or "Customer"
    }
}

