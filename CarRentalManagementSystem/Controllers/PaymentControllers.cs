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


   

            // --- UPDATED GET ACTION ---
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

            // --- UPDATED POST ACTION ---
            // POST: Payment/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(PaymentViewModel viewModel)
            {
                // Credit card validation logic (remains the same)
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

                    // Create the new Booking object here, at the final step
                    var newBooking = new Booking
                    {
                        CarID = viewModel.CarID,
                        CustomerID = customerId.Value,
                        PickupDate = viewModel.PickupDate,
                        ReturnDate = viewModel.ReturnDate,
                        TotalCost = viewModel.Amount,
                        // Status will be set below based on payment method
                    };

                    // Logic based on the selected payment method
                    if (viewModel.PaymentMethod == "Credit Card")
                    {
                        // Dummy validation for declined card
                        if (viewModel.CardNumber.EndsWith("0000"))
                        {
                            ModelState.AddModelError("", "Your card was declined. Please try a different card.");
                            return await PrepareViewModelForError(viewModel);
                        }

                        newBooking.Status = "Paid";
                        _context.Bookings.Add(newBooking);

                        var newPayment = new Payment
                        {
                            Booking = newBooking, // Link to the booking we just created
                            Amount = viewModel.Amount,
                            PaymentMethod = viewModel.PaymentMethod,
                            PaymentDate = DateTime.Now,
                            PaymentStatus = "Completed",
                            TransactionID = $"TXN-{DateTime.UtcNow.Ticks}"
                        };
                        _context.Payments.Add(newPayment);

                        await _context.SaveChangesAsync();
                        return RedirectToAction("Success", new { bookingId = newBooking.BookingID });
                    }
                    else if (viewModel.PaymentMethod == "Cash on Pickup")
                    {
                        newBooking.Status = "Pending";
                        _context.Bookings.Add(newBooking);

                        await _context.SaveChangesAsync();
                        // Redirect to a new confirmation page for pending bookings
                        return RedirectToAction("PendingConfirmation", new { bookingId = newBooking.BookingID });
                    }
                }

                // If ModelState is invalid, prepare the view model again and return to the view
                return await PrepareViewModelForError(viewModel);
            }

            // --- UPDATED HELPER METHOD ---
            // This helper no longer relies on a BookingID
            private async Task<IActionResult> PrepareViewModelForError(PaymentViewModel viewModel)
            {
                var carForDisplay = await _context.Cars.FindAsync(viewModel.CarID);
                viewModel.CarName = carForDisplay?.CarName;
                viewModel.CarModel = carForDisplay?.Model;
                // PickupDate and ReturnDate are already in the viewModel
                viewModel.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" },
                new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Pay at Desk)" }
            };
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
    }
}