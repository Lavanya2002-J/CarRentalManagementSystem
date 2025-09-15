using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{

    public class CarViewModel
    {
        [Required]
        public Guid CarID { get; set; }

        [Required(ErrorMessage = "Car name is required")]
        [StringLength(100, ErrorMessage = "Car name can't exceed 100 characters")]
        [Display(Name = "Car Name")]
        public string CarName { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50, ErrorMessage = "Car model can't exceed 50 characters")]
        [Display(Name = "Model")]

        public string Model { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [StringLength(20)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Required(ErrorMessage = "Transmission is required")]
        [StringLength(20)]
        public string Transmission { get; set; }

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(1, 15, ErrorMessage = "Seats must be between 1 and 15")]
        public int Seats { get; set; }

        
        [Range(500, 200000, ErrorMessage = "Daily rate must be a valid amount")]
        [Display(Name = "Daily Rate (LKR)")]
        public decimal DailyRate { get; set; }


        [Display(Name = "Is Available for Rent?")]
        public bool IsAvailable { get; set; } = true;

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters")]
        public string Description { get; set; }

        [StringLength(30, ErrorMessage = "Color can't exceed 30 characters")]
        public string Color { get; set; }

        [StringLength(50, ErrorMessage = "Registration number can't exceed 50 characters")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }



        [Required(ErrorMessage = "Fuel capacity is required")]
        [Range(10, 200, ErrorMessage = "Fuel capacity must be between 10 and 200 litres")]
        [Display(Name = "Fuel Capacity (Litres)")]
        public int FuelCapacity { get; set; }

        [StringLength(50)]
        [Display(Name = "Insurance Policy No.")]
        public string InsurancePolicyNo { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Insurance Expiry Date")]
        public DateTime? InsuranceExpiryDate { get; set; }


        // --- File Uploads & Existing Paths ---

        [Display(Name = "Brand Logo")]
        public IFormFile LogoFile { get; set; }

        [Display(Name = "Car Image")]
        public IFormFile CarImageFile { get; set; }

        public string ExistingLogoPath { get; set; }
        public string ExistingCarImagePath { get; set; }


    }
}