using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class FeedbackViewModel
    {
        // To identify the booking
        public int BookingID { get; set; }

        // To create the Feedback object
        public int CustomerID { get; set; }
        public Guid CarID { get; set; }

        // For display purposes on the form
        public string CarName { get; set; }
        public string CarModel { get; set; }
        public string CustomerName { get; set; }

        // Form fields
        [Required]
        public string Ratings { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Feedback cannot be longer than 500 characters.")]
        public string FeedBack { get; set; }
    }
}
