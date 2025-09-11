using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class PaymentViewModel
    {
        // From Booking (usually passed as hidden fields)
        public int BookingID { get; set; }

        [Display(Name = "Total Amount")]
        public decimal Amount { get; set; }

        // Card details (for payment processing)
        [Required(ErrorMessage = "Card Number is required")]
        [CreditCard(ErrorMessage = "Invalid Credit Card Number")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Name on Card is required")]
        [Display(Name = "Name on Card")]
        public string NameOnCard { get; set; }

        [Required(ErrorMessage = "Expiry Month is required")]
        [Range(1, 12, ErrorMessage = "Invalid Month (1-12)")]
        [Display(Name = "Expiry Month")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry Year is required")]
        [Range(2024, 2030, ErrorMessage = "Invalid Year")]
        [Display(Name = "Expiry Year")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3-4 digits")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        // Optional: Save card for future payments
        [Display(Name = "Save card for future payments")]
        public bool SaveCard { get; set; }
    }
}

