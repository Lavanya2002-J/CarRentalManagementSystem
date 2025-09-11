using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CarRentalManagementSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method for role-based access control
        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("Role") == "Customer";
        }

        // GET: Booking/Create/{carId} - Displays the booking form
        public IActionResult Create(Guid carId)
        {
            // Restrict access to customers only
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in as a customer to make a booking.";
                return RedirectToAction("Login", "Account");
            }

            // Find the selected car
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            if (car == null)
            {
                return NotFound();
            }

            // Check if the car is available for booking
            if (car.IsAvailable != "Yes")
            {
                TempData["ErrorMessage"] = "This car is currently not available for booking.";
                return RedirectToAction("Index", "Home");
            }

            // Pass the car details to the view
            ViewBag.Car = car;
            return View(new Booking());
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking model)
        {
            // Restrict access to customers only
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in as a customer to make a booking.";
                return RedirectToAction("Login", "Account");
            }

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == model.CarID);
            if (car == null)
            {
                return NotFound();
            }

            // Business logic and validation as per BRD
            if (model.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date must be today or a future date.");
            }

            if (model.ReturnDate <= model.PickupDate)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after the pickup date.");
            }

            if (ModelState.IsValid)
            {
                // Calculate the total cost based on the number of days
                var rentalDays = (model.ReturnDate - model.PickupDate).TotalDays;
                model.TotalCost = (decimal)rentalDays * car.DailyRate;

                // Get CustomerID from session
                model.CustomerID = HttpContext.Session.GetInt32("UserID").Value;
                model.Status = "Pending"; // Set initial status

                _context.Bookings.Add(model);

                // Update car availability to "No"
                car.IsAvailable = "No";
                _context.Cars.Update(car);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your booking has been created successfully!";
                return RedirectToAction("History");
            }

            // If model state is not valid, return to the form with errors
            ViewBag.Car = car;
            return View(model);
        }

        // GET: Booking/History - Displays a customer's booking history
        public async Task<IActionResult> History()
        {
            // Restrict access to customers only
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in as a customer to view your booking history.";
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the current customer's ID from the session
            var customerId = HttpContext.Session.GetInt32("UserID");

            // Retrieve all bookings for the customer, including car details
            var customerBookings = await _context.Bookings
        .Where(b => b.CustomerID == customerId)
        .Include(b => b.Car) // <-- Add this line to load the related Car data
        .ToListAsync();

            return View(customerBookings);
        }
    }
}
