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
    }
}