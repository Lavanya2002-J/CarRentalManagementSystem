using System;
using System.ComponentModel.DataAnnotations;

    namespace CarRentalManagementSystem.Models
    {
    public class Car
    {
        public Guid CarId { get; set; }
        public string CarName { get; set; }
        public string CarModel { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public int Seats { get; set; }
        public decimal DailyRate { get; set; }
        public string IsAvailable { get; set; }
        public string Branch { get; set; }

        public string LogoFileName { get; set; }
        public string CarImageFileName { get; set; }

        // Optional fields
        public string Description { get; set; }
        public string Color { get; set; }
        public string RegistrationNumber { get; set; }

        
        public ICollection<Booking> Bookings { get; set; }
    }

    
}







//{
//    public class Car

//        {
//            [Key]
//            public int CarID { get; set; }

//            [Required]
//            [StringLength(100)]
//            public string CarName { get; set; }

//            [Required]
//            [StringLength(50)]
//            public string Model { get; set; }

//            [Required]
//            [StringLength(20)]
//            public string FuelType { get; set; }

//            [Required]
//            [StringLength(20)]
//            public string Transmission { get; set; }

//            [StringLength(500)]
//            public string Description { get; set; }

//            [Required]
//            public int Seats { get; set; }

//            [Required]
//            [Column(TypeName = "decimal(10,2)")]
//            public decimal DailyRate { get; set; }

//            public bool IsAvailable { get; set; } = true;

//            [StringLength(20)]
//            public string Color { get; set; }

//            [StringLength(50)]
//            public string RegistrationNumber { get; set; }

//            public int FuelCapacity { get; set; }

//            [StringLength(50)]
//            public string InsurancePolicyNo { get; set; }

//            public DateTime? InsuranceExpiryDate { get; set; }

//            public string LogoFileName { get; set; }

//             public string CarImageFileName { get; set; }

//             // Navigation
//             public ICollection<Booking> Bookings { get; set; }


//    }
//    }

