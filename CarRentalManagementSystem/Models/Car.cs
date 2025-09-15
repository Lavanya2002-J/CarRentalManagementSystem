using CarRentalManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CarRentalManagementSystem.Models
{

        public class Car
        {
            [Key] 
            public Guid CarID { get; set; }

            [Required] 
            [StringLength(100)] 
            public string CarName { get; set; }

            [Required]
            [StringLength(50)]
            public string Model { get; set; } 

            [Required]
            [StringLength(20)]
            public string FuelType { get; set; }

            [Required]
            [StringLength(20)]
            public string Transmission { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            [Required]
            public int Seats { get; set; }

            
            [Column(TypeName = "decimal(10,2)")] 
            public decimal DailyRate { get; set; }

    
            public bool IsAvailable { get; set; } = true; 

            [StringLength(20)]
            public string Color { get; set; }

            [StringLength(50)]
            public string RegistrationNumber { get; set; }

    
            public int FuelCapacity { get; set; }

            [StringLength(50)]
            public string InsurancePolicyNo { get; set; }

            public DateTime? InsuranceExpiryDate { get; set; } 

            public string LogoFileName { get; set; }

            public string CarImageFileName { get; set; }

            // --- Navigation Property 
            public ICollection<Booking> Bookings { get; set; }
        }
}