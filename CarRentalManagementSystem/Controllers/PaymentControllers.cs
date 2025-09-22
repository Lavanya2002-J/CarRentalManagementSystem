using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Payment or /Payment/Index
        public async Task<IActionResult> Index()
        {

            var payments = await _context.Payments
                                         .OrderByDescending(p => p.PaymentDate)
                                         .ToListAsync();
            return View(payments);
        }


              // GET: Payment/Create
            [HttpGet]
            public async Task<IActionResult> Create(Guid carId, DateTime pickupDate, DateTime returnDate)
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null) return NotFound("Car not found.");

                // Calculate rental days and total cost on the server
                var rentalDays = (returnDate - pickupDate).TotalDays;
                if (rentalDays <= 0) rentalDays = 1;
                var totalCost = (decimal)rentalDays * car.DailyRate;

                // Build the ViewModel with details for the new booking
                var viewModel = new PaymentViewModel
                {
                    CarID = carId, // Pass CarID to the view
                    PickupDate = pickupDate,
                    ReturnDate = returnDate,
                    Amount = totalCost,
                    CarName = car.CarName,
                    CarModel = car.Model,
                    PaymentMethods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" },
                    new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Pay at Desk)" }
                }
                };
                return View(viewModel);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentViewModel viewModel)
        {
            // --- Step 1: Credit card field validation (no changes here) ---
            if (viewModel.PaymentMethod == "Credit Card")
            {
                if (string.IsNullOrWhiteSpace(viewModel.CardHolderName))
                    ModelState.AddModelError("CardHolderName", "Cardholder Name is required.");
                if (string.IsNullOrWhiteSpace(viewModel.CardNumber))
                    ModelState.AddModelError("CardNumber", "Card Number is required.");
                if (string.IsNullOrWhiteSpace(viewModel.ExpiryDate))
                    ModelState.AddModelError("ExpiryDate", "Expiry Date is required.");
                if (string.IsNullOrWhiteSpace(viewModel.Cvc))
                    ModelState.AddModelError("Cvc", "CVC is required.");
            }

            if (ModelState.IsValid)
            {
                var customerId = HttpContext.Session.GetInt32("UserID");
                if (customerId == null) return RedirectToAction("CustomerLogin", "Account");

                // --- Step 2: Handle the two different booking scenarios ---
                Booking bookingToUpdate;
                int bookingIdForRedirect;

                if (viewModel.BookingID > 0)
                {
                    // --- SCENARIO A: Paying for an EXISTING "Pending" booking ---

                    // Find the booking the customer is trying to pay for
                    bookingToUpdate = await _context.Bookings.FindAsync(viewModel.BookingID);
                    if (bookingToUpdate == null || bookingToUpdate.CustomerID != customerId)
                    {
                        return Forbid(); // Prevent users from paying for bookings that aren't theirs
                    }

                    // Update its status to "Paid"
                    bookingToUpdate.Status = "Paid";
                    bookingIdForRedirect = bookingToUpdate.BookingID;
                }
                else
                {
                    // --- SCENARIO B: Creating a NEW booking (Cash or Card) ---

                    // Create a brand new booking object from the view model
                    var newBooking = new Booking
                    {
                        CarID = viewModel.CarID,
                        CustomerID = customerId.Value,
                        PickupDate = viewModel.PickupDate,
                        ReturnDate = viewModel.ReturnDate,
                        TotalCost = viewModel.Amount
                    };

                    if (viewModel.PaymentMethod == "Cash on Pickup")
                    {
                        newBooking.Status = "Pending";
                        _context.Bookings.Add(newBooking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("PendingConfirmation", new { bookingId = newBooking.BookingID });
                    }

                    // If it's a card payment, set status to "Paid"
                    newBooking.Status = "Paid";
                    _context.Bookings.Add(newBooking);
                    bookingToUpdate = newBooking; // Set it as the booking to attach payment to
                    bookingIdForRedirect = 0; // Will get ID after saving
                }

                // --- Step 3: Create the Payment record for card transactions ---
                if (viewModel.PaymentMethod == "Credit Card")
                {
                    var newPayment = new Payment
                    {
                        Booking = bookingToUpdate, // Link to either the new or existing booking
                        Amount = viewModel.Amount,
                        PaymentMethod = viewModel.PaymentMethod,
                        PaymentDate = DateTime.Now,
                        PaymentStatus = "Completed",
                        TransactionID = $"TXN-{DateTime.UtcNow.Ticks}"
                    };
                    _context.Payments.Add(newPayment);
                }

                await _context.SaveChangesAsync();

                // If it was a new booking, its ID is now generated
                if (bookingIdForRedirect == 0)
                {
                    bookingIdForRedirect = bookingToUpdate.BookingID;
                }

                return RedirectToAction("Success", new { bookingId = bookingIdForRedirect });
            }

            // If ModelState is invalid, prepare the view model and return to the view
            return await PrepareViewModelForError(viewModel);
        }
        // In Controllers/PaymentController.cs

        private async Task<IActionResult> PrepareViewModelForError(PaymentViewModel viewModel)
        {
            // 1. Find the car using the CarID from the view model to get its name and model for display.
            var carForDisplay = await _context.Cars.FindAsync(viewModel.CarID);
            viewModel.CarName = carForDisplay?.CarName;
            viewModel.CarModel = carForDisplay?.Model;

            // 2. Re-populate the dropdown list for payment methods.
            viewModel.PaymentMethods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" },
        new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Pay at Desk)" }
    };

            // 3. Return the user to the "Create" view, passing back the corrected view model.
            //    This ensures they see their original selections and the validation error message.
            return View("Create", viewModel);
        }

        // GET: Payment/Success
        public async Task<IActionResult> Success(int bookingId)
        {

            var booking = await _context.Bookings
                                        .Include(b => b.Car)
                                        .Include(b => b.Customer)
                                        .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.SuccessMessage = "Your booking has been confirmed successfully!";
            return View(booking);
        }

        // GET: Payment/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Car)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(m => m.PaymentID == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
        

        // GET: Payment/PendingConfirmation/5
        public async Task<IActionResult> PendingConfirmation(int bookingId)
        {
            var booking = await _context.Bookings
                                        .Include(b => b.Car)
                                        .FirstOrDefaultAsync(b => b.BookingID == bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            // This ViewBag message will be displayed on the confirmation page
            ViewBag.SuccessMessage = "Your car has been reserved! Please complete the payment at the counter on your pickup day.";
            return View(booking);
        }
        // In Controllers/PaymentController.cs

        // GET: /Payment/PayForBooking/5
        public async Task<IActionResult> PayForBooking(int bookingId)
        {
            var customerId = HttpContext.Session.GetInt32("UserID");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            // Find the booking and include the car details
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == customerId.Value);

            if (booking == null)
            {
                return NotFound("Booking not found or you do not have permission to view it.");
            }

            // A user should only be able to pay for a pending booking
            if (booking.Status != "Pending" && booking.Status != "Pending Payment")
            {
                TempData["ErrorMessage"] = "This booking is not awaiting payment.";
                return RedirectToAction("History", "Booking");
            }

            // Create the ViewModel using the existing booking's data
            var viewModel = new PaymentViewModel
            {
                BookingID = booking.BookingID, // IMPORTANT: Pass the existing BookingID
                CarID = booking.CarID,
                Amount = booking.TotalCost,
                CarName = booking.Car.CarName,
                CarModel = booking.Car.Model,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                PaymentMethods = new List<SelectListItem>
        {
            new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" }
            // Note: "Cash on Pickup" is removed as this is an online payment flow
        }
            };

            // Return the standard "Create" view, but with the data from the existing booking
            return View("Create", viewModel);
        }

    }
}