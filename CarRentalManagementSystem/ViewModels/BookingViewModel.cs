using System;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class BookingViewModel
    {
        // --- Data to be submitted by the form ---

        [Required]
        public Guid CarID { get; set; }

        [Required(ErrorMessage = "Please select a pickup date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        public DateTime PickupDate { get; set; } = DateTime.Today; // Default to today

        [Required(ErrorMessage = "Please select a return date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(1); // Default to tomorrow

        // --- Data to be displayed on the page (read-only) ---

        public string CarName { get; set; }
        public string CarModel { get; set; }
        public decimal DailyRate { get; set; }
        public string CarImageFileName { get; set; }
        public int Seats { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }

        // This property will be used to display validation messages from the controller
        public string ErrorMessage { get; set; }
    }
}
