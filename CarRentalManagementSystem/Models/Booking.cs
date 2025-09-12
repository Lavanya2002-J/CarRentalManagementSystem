using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // <-- Make sure System is included

namespace CarRentalManagementSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        [Required]
        [ForeignKey("Car")]
        public Guid CarID { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public decimal TotalCost { get; set; }

        // Add navigation properties
        public virtual Customer Customer { get; set; }
        public virtual Car Car { get; set; }
    }
}