using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class PaymentViewModel
    {
        // Data for creating the Payment record
        public int BookingID { get; set; }
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        // Data to DISPLAY on the page (from Booking and Car models)
        public string CarName { get; set; }
        public string CarModel { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }

        // For the dropdown list in the view
        public IEnumerable<SelectListItem> PaymentMethods { get; set; }

        // -- Card Payment-kaana Puthu Properties --
        [Display(Name = "Cardholder Name")]
        public string CardHolderName { get; set; }

        [Display(Name = "Card Number")]
        [CreditCard(ErrorMessage = "Please enter a valid card number.")]
        public string CardNumber { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Invalid format. Use MM/YY.")]
        public string ExpiryDate { get; set; }

        [Display(Name = "CVC")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVC.")]
        public string Cvc { get; set; }


    }
}