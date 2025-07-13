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
        }
    } 