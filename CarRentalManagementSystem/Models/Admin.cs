using CarRentalManagementSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class Admin
    {
        [Key]
        public Guid AdminID { get; set; }

        [Required]
        [StringLength(100)]

        public string Name { get; set; }


        [Required]
        [EmailAddress]
        
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [CustomPassword]
        public string Password { get; set; }
    }
}
