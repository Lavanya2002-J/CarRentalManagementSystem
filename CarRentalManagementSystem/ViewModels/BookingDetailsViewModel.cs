using CarRentalManagementSystem.Models;
using System.Collections.Generic;

namespace CarRentalManagementSystem.ViewModels
{
    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
