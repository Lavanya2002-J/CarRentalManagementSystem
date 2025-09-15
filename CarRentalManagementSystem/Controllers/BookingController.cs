

using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels; // <-- Add this using statement
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

            // Create and populate the ViewModel instead of using ViewBag
            var viewModel = new BookingViewModel
            {
                CarID = car.CarId,
                CarName = car.CarName,
                CarModel = car.CarModel,
                DailyRate = car.DailyRate,
                CarImageFileName = car.CarImageFileName,
                Seats = car.Seats,
                FuelType = car.FuelType,
                Transmission = car.Transmission
            };

            return View(viewModel);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel viewModel)
        {
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            var car = await _context.Cars.FindAsync(viewModel.CarID);
            if (car == null)
            {
                ModelState.AddModelError("", "The selected car could not be found.");
            }
            else if (car.IsAvailable != "Yes")
            {
                TempData["ErrorMessage"] = "Sorry, this car was booked by another user while you were deciding. Please choose another car.";
                return RedirectToAction("Index", "Home");
            }

            // --- Additional Server-side Validation ---
            if (viewModel.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");
            }

            if (viewModel.ReturnDate <= viewModel.PickupDate)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after the pickup date.");
            }


            // ✅ **BUG FIX:** The logic is now correctly placed inside 'if (ModelState.IsValid)'
            if (ModelState.IsValid)
            {
                var rentalDays = (viewModel.ReturnDate - viewModel.PickupDate).TotalDays;
                if (rentalDays <= 0) rentalDays = 1; // Ensure at least one day is charged

                // Create a new Booking domain model from the ViewModel
                var newBooking = new Booking
                {
                    CarID = viewModel.CarID,
                    PickupDate = viewModel.PickupDate,
                    ReturnDate = viewModel.ReturnDate,
                    TotalCost = (decimal)rentalDays * car.DailyRate,
                    CustomerID = HttpContext.Session.GetInt32("UserID").Value,
                    Status = "Pending" // Initial status before payment
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // Redirect to payment with the newly created BookingID
                return RedirectToAction("Create", "Payment", new { bookingId = newBooking.BookingID });
            }

            // If the model is NOT valid, we must re-populate the car details for display
            if (car != null)
            {
                viewModel.CarName = car.CarName;
                viewModel.CarModel = car.CarModel;
                viewModel.DailyRate = car.DailyRate;
                viewModel.CarImageFileName = car.CarImageFileName;
                viewModel.Seats = car.Seats;
                viewModel.FuelType = car.FuelType;
                viewModel.Transmission = car.Transmission;
            }

            return View(viewModel); // Return the view with validation errors
        }

        // ... (The rest of your controller methods: History, Details, etc. remain the same)
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
