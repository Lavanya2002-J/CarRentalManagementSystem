
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCustomer()
        {
            return HttpContext.Session.GetString("Role") == "Customer";
        }

        // GET: Booking/Create/{carId}
        public async Task<IActionResult> Create(Guid carId)
        {
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in to make a booking.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            if (car.IsAvailable != "Yes")
            {
                TempData["ErrorMessage"] = "This car is no longer available for booking.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Car = car;
            return View(new Booking());
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking model)
        {
            // Re-fetch car details for security and consistency
            var car = await _context.Cars.FindAsync(model.CarID);

            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            if (car == null)
            {
                ModelState.AddModelError("", "The selected car could not be found.");
            }
            // Check if car is still available at the time of submission
            else if (car.IsAvailable != "Yes")
            {
                TempData["ErrorMessage"] = "Sorry, this car was booked by another user while you were deciding. Please choose another car.";
                return RedirectToAction("Index", "Home");
            }

            // Server-side validation for dates
            if (model.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");
            }

            if (model.ReturnDate <= model.PickupDate)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after the pickup date.");
            }

            if (!ModelState.IsValid)
            {
                var rentalDays = (model.ReturnDate - model.PickupDate).TotalDays;
                if (rentalDays <= 0) rentalDays = 1; // Ensure at least one day is charged

                model.TotalCost = (decimal)rentalDays * car.DailyRate;
                model.CustomerID = HttpContext.Session.GetInt32("UserID").Value;
                model.Status = "Pending";

                _context.Bookings.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Create", "Payment", new { bookingId = model.BookingID, amount = model.TotalCost });
            }

            // If the model is invalid, return to the form with the car details and error messages
            ViewBag.Car = car;
            return View(model);
        }

        // GET: Booking/History
        public async Task<IActionResult> History()
        {
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in to view your booking history.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("UserID");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerBookings = await _context.Bookings
                .Where(b => b.CustomerID == customerId.Value)
                .Include(b => b.Car)
                .OrderByDescending(b => b.PickupDate)
                .ToListAsync();

            return View(customerBookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You are not authorized to view this page.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("UserID").Value;

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == customerId);

            if (booking == null)
            {
                return NotFound();
            }
            ViewBag.Username = HttpContext.Session.GetString("Username");
           

            return View(booking);
        }
    }
}
