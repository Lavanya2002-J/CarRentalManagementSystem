using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarRentalManagementSystem.ViewModels
{
    public class AdminBookingViewModel
    {
        [Required(ErrorMessage = "Please select a customer.")]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Please select a car.")]
        [Display(Name = "Car")]
        public Guid CarID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        public DateTime PickupDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today.AddDays(1);

        // Properties to hold the dropdown lists
        public IEnumerable<SelectListItem> Customers { get; set; }
        public IEnumerable<SelectListItem> Cars { get; set; }
    }
}