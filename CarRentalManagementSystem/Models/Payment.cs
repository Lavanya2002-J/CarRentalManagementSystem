using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int BookingID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }

        // Add this new property
        [StringLength(50)]
        public string TransactionID { get; set; }
        public virtual Booking Booking { get; set; }

    }
}
