using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Booking/Create/{carId}
        public async Task<IActionResult> Create(Guid carId)
        {
            if (!IsCustomer())
            {
                TempData["ErrorMessage"] = "You must be logged in to make a booking.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            // Fetch the car
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
                return NotFound();

            // Global availability check (maintenance or removed car)
            if (!car.IsAvailable)
            {
                TempData["ErrorMessage"] = "This car is no longer available for booking.";
                return RedirectToAction("Index", "Home");
            }

            // Prepare ViewModel
            var viewModel = new BookingViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                CarModel = car.Model,
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

            // Fetch the car
            var car = await _context.Cars.FindAsync(viewModel.CarID);
            if (car == null)
                return NotFound();

            // Server-side validation
            if (viewModel.PickupDate < DateTime.Today)
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");

            if (viewModel.ReturnDate <= viewModel.PickupDate)
                ModelState.AddModelError("ReturnDate", "Return date must be after the pickup date.");

            // Check for overlapping bookings
            bool hasOverlap = await _context.Bookings.AnyAsync(b =>
                b.CarID == viewModel.CarID &&
                (b.Status == "Paid" || b.Status == "Pending") &&
                b.PickupDate < viewModel.ReturnDate &&
                b.ReturnDate > viewModel.PickupDate);

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Sorry, this car is already booked for the selected dates.");

                // Re-populate car details in ViewModel
                viewModel.CarName = car.CarName;
                viewModel.CarModel = car.Model;
                viewModel.DailyRate = car.DailyRate;
                viewModel.CarImageFileName = car.CarImageFileName;
                viewModel.Seats = car.Seats;
                viewModel.FuelType = car.FuelType;
                viewModel.Transmission = car.Transmission;

                return View(viewModel); // Stay on Booking page
            }

            if (ModelState.IsValid)
            {
                // Calculate rental days
                var rentalDays = (viewModel.ReturnDate - viewModel.PickupDate).TotalDays;
                if (rentalDays <= 0) rentalDays = 1;

                // Create new booking
                var newBooking = new Booking
                {
                    CarID = viewModel.CarID,
                    PickupDate = viewModel.PickupDate,
                    ReturnDate = viewModel.ReturnDate,
                    TotalCost = (decimal)rentalDays * car.DailyRate,
                    CustomerID = HttpContext.Session.GetInt32("UserID").Value,
                    Status = "Pending"
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                return RedirectToAction("Create", "Payment", new { bookingId = newBooking.BookingID });
            }

            // Re-populate ViewModel to show in view in case of errors
            viewModel.CarName = car.CarName;
            viewModel.CarModel = car.Model;
            viewModel.DailyRate = car.DailyRate;
            viewModel.CarImageFileName = car.CarImageFileName;
            viewModel.Seats = car.Seats;
            viewModel.FuelType = car.FuelType;
            viewModel.Transmission = car.Transmission;

            return View(viewModel);
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

            // --- NEW: Fetches all payments (including refunds) for this booking ---
            var payments = await _context.Payments
                .Where(p => p.BookingID == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            // --- NEW: Creates and populates the ViewModel ---
            var viewModel = new BookingDetailsViewModel
            {
                Booking = booking,
                Payments = payments
            };

            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View(viewModel); // Pass the new ViewModel to the view
        }


        //// GET: Booking/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // Ensure the user is a logged-in customer
            if (!IsCustomer())
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("UserID").Value;

            // Find the booking and include car details
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == customerId);

            if (booking == null)
            {
                return NotFound(); // Booking not found or doesn't belong to the user
            }

            // A user can't cancel a booking that's already cancelled or completed
            if (booking.Status != "Pending" && booking.Status != "Paid")
            {
                TempData["ErrorMessage"] = "This booking cannot be cancelled.";
                return RedirectToAction(nameof(History));
            }

            return View(booking);
        }



        // POST: Booking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            // Ensure the user is a logged-in customer
            if (!IsCustomer())
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("UserID").Value;

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == customerId);

            if (booking == null)
            {
                return NotFound();
            }

            // --- START: Updated Logic for Admin Approval ---
            // This method no longer creates a refund payment or makes the car available.
            // It simply updates the status to flag the booking for admin review.
            booking.Status = "Cancellation Requested";

            await _context.SaveChangesAsync();

            // The success message now informs the user about the new approval process.
            TempData["SuccessMessage"] = "Your cancellation request has been submitted for admin approval.";
            return RedirectToAction(nameof(History));
            // --- END: Updated Logic for Admin Approval ---
        }


        // GET: Booking/CreateByAdmin
        public async Task<IActionResult> CreateByAdmin()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new AdminBookingViewModel
            {
                Customers = await _context.Customers
                    .OrderBy(c => c.CustomerName)
                    .Select(c => new SelectListItem { Value = c.CustomerID.ToString(), Text = c.CustomerName + " (" + c.mail + ")" })
                    .ToListAsync(),

                // --- CHANGE: The .Where() clause is removed to show all cars ---
                Cars = await _context.Cars
                    .OrderBy(c => c.CarName)
                    .Select(car => new SelectListItem { Value = car.CarID.ToString(), Text = car.CarName + " - " + car.Model })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Booking/CreateByAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByAdmin(AdminBookingViewModel viewModel)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Server-side validation for dates
            if (viewModel.ReturnDate <= viewModel.PickupDate)
            {
                ModelState.AddModelError("ReturnDate", "Return date must be after the pickup date.");
            }

            if (viewModel.PickupDate < DateTime.Today)
            {
                ModelState.AddModelError("PickupDate", "Pickup date cannot be in the past.");
            }

            // --- NEW: Dynamic check for overlapping bookings ---
            bool hasOverlap = await _context.Bookings.AnyAsync(b =>
                b.CarID == viewModel.CarID &&
                (b.Status == "Paid" || b.Status == "Pending") && // Check against active bookings
                b.PickupDate < viewModel.ReturnDate &&
                b.ReturnDate > viewModel.PickupDate);

            if (hasOverlap)
            {
                ModelState.AddModelError("", "This car is already booked for the selected dates. Please choose different dates or another car.");
            }
            // --- End of new check ---

            if (ModelState.IsValid)
            {
                var car = await _context.Cars.FindAsync(viewModel.CarID);
                if (car == null)
                {
                    // This case should ideally not happen if the form is working correctly
                    TempData["ErrorMessage"] = "The selected car could not be found.";
                    return RedirectToAction(nameof(CreateByAdmin));
                }

                var rentalDays = (viewModel.ReturnDate - viewModel.PickupDate).TotalDays;
                if (rentalDays <= 0) rentalDays = 1; // Ensure at least one day is charged

                // 1. Create the new booking
                var newBooking = new Booking
                {
                    CustomerID = viewModel.CustomerID,
                    CarID = viewModel.CarID,
                    PickupDate = viewModel.PickupDate,
                    ReturnDate = viewModel.ReturnDate,
                    TotalCost = (decimal)rentalDays * car.DailyRate,
                    Status = "Paid" // Admin bookings are considered paid immediately
                };
                _context.Bookings.Add(newBooking);

                // --- REMOVED: The line "car.IsAvailable = false;" is no longer here ---

                // 2. Create the corresponding payment record
                var newPayment = new Payment
                {
                    Booking = newBooking,
                    Amount = newBooking.TotalCost,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Admin Entry",
                    PaymentStatus = "Completed",
                    TransactionID = $"ADM-TXN-{DateTime.UtcNow.Ticks}"
                };
                _context.Payments.Add(newPayment);

                // 3. Save both changes to the database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking and Payment created successfully!";
                return RedirectToAction(nameof(ViewAllBookings));
            }

            // If the model is invalid, re-populate the dropdown lists before showing the page again
            viewModel.Customers = await _context.Customers
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem { Value = c.CustomerID.ToString(), Text = c.CustomerName + " (" + c.mail + ")" })
                .ToListAsync();
            viewModel.Cars = await _context.Cars
                .OrderBy(c => c.CarName)
                .Select(car => new SelectListItem { Value = car.CarID.ToString(), Text = car.CarName + " - " + car.Model })
                .ToListAsync();

            return View(viewModel);
        }


        //// GET: Booking/ViewAllBookings
        public async Task<IActionResult> ViewAllBookings()
        {
            // Role-based access check
            if (!IsAdmin())
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // Fetch all bookings from the database
            var allBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingID)
                .ToListAsync();

            return View(allBookings);
        }
    }
}





