using CarRentalManagementSystem.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [CustomEmail]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [StringLength(50)]
        [CustomPassword]
        public string Password { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [StringLength(200)]
        [Required]
        public string Address { get; set; }
        [Required]
        [CustomNic]
        public string NIC { get; set; }
        [Required]
        [CustomLicence]
        public string LicenseNo { get; set; }
        public ICollection<Booking> Bookings { get; set; } 



    }
}

