using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.Models.ViewModels
{
  
        public class CarViewModel
        {
            public Guid CarId { get; set; }

            [Required(ErrorMessage = "Car name is required")]
            [StringLength(50, ErrorMessage = "Car name can't exceed 50 characters")]
            public string CarName { get; set; }

            [Required(ErrorMessage = "Model is required")]
            [StringLength(30, ErrorMessage = "Car model can't exceed 30 characters")]
            public string CarModel { get; set; }

            [Required(ErrorMessage = "Fuel type is required")]
            public string FuelType { get; set; }

            [Required(ErrorMessage = "Transmission is required")]
            public string Transmission { get; set; }

            [Range(1, 10, ErrorMessage = "Seats must be between 1 and 10")]
            public int Seats { get; set; }

            [Range(100, 100000, ErrorMessage = "Daily rate must be between 100 and 100000")]
            public decimal DailyRate { get; set; }

            [Required(ErrorMessage = "Availability is required")]
            [RegularExpression("Yes|No", ErrorMessage = "Only 'Yes' or 'No' allowed")]
            public string IsAvailable { get; set; }

            [Required(ErrorMessage = "Branch is required")]
            [StringLength(50)]
            public string Branch { get; set; }

            public string ExistingLogoPath { get; set; }
            public string ExistingCarImagePath { get; set; }

        // File uploads
        public IFormFile LogoFile { get; set; }

            public IFormFile CarImageFile { get; set; }

            // Optional fields with validation
            [StringLength(200, ErrorMessage = "Description can't exceed 200 characters")]
            public string Description { get; set; }

            [StringLength(20, ErrorMessage = "Color can't exceed 20 characters")]
            public string Color { get; set; }

            [StringLength(20, ErrorMessage = "Registration number can't exceed 20 characters")]
            public string RegistrationNumber { get; set; }
        }
    
}
