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
        // GET: Payment/Create
        [HttpGet]
        public async Task<IActionResult> Create(int bookingId, decimal amount)
        {
            var booking = await _context.Bookings
                                        .Include(b => b.Car)
                                        .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null) return NotFound("Booking not found.");

            var viewModel = new PaymentViewModel
            {
                BookingID = booking.BookingID,
                Amount = booking.TotalCost,
                CarName = booking.Car.CarName,
                CarModel = booking.Car.Model,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                PaymentMethods = new List<SelectListItem>
                {
                    // Dropdown-kaana options
                    new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Manual)" },
                    new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" }
                }
            };
            return View(viewModel);
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentViewModel viewModel)
        {
            // Payment method "Credit Card" aaga irunthu, card details thevai-naa, ModelState-ah check pannum mun validation rules-ah serkkanum
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
                var booking = await _context.Bookings.FindAsync(viewModel.BookingID);
                if (booking == null) return NotFound();

                // Payment method-ai poruthu logic-ah pirikkavum
                if (viewModel.PaymentMethod == "Credit Card")
                {
                    // Dummy validation: Oru test card number-ah mattum fail aakkalam
                    if (viewModel.CardNumber.EndsWith("0000"))
                    {
                        ModelState.AddModelError("", "Your card was declined. Please try a different card.");
                        // Error iruppathal, view-ku thirumba pogum mun display properties-ah load pannunga
                        return await PrepareViewModelForError(viewModel);
                    }
                    // Vetrigaramaaga "process" aanathaaga karuthikkollavum
                }

                // --- Common Logic for both Cash and successful Card payment ---
                var car = await _context.Cars.FindAsync(booking.CarID);
                if (car == null) return NotFound();

                var newPayment = new Payment
                {
                    BookingID = viewModel.BookingID,
                    Amount = viewModel.Amount,
                    PaymentMethod = viewModel.PaymentMethod,
                    PaymentDate = System.DateTime.Now,
                    PaymentStatus = "Completed",
                    TransactionID = $"TXN-{System.DateTime.UtcNow.Ticks}"
                };

                _context.Payments.Add(newPayment);
                booking.Status = "Paid";
                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { bookingId = booking.BookingID });
            }

            // ModelState valid illai-naa, view-ku thirumba pogum mun display properties-ah load pannunga
            return await PrepareViewModelForError(viewModel);
        }

        // Oru chinna helper method, code-ah repeat seiyaamal irukka
        private async Task<IActionResult> PrepareViewModelForError(PaymentViewModel viewModel)
        {
            var bookingForDisplay = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.BookingID == viewModel.BookingID);
            viewModel.CarName = bookingForDisplay.Car.CarName;
            viewModel.CarModel = bookingForDisplay.Car.Model;
            viewModel.PickupDate = bookingForDisplay.PickupDate;
            viewModel.ReturnDate = bookingForDisplay.ReturnDate;
            viewModel.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Manual)" },
                new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" }
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
    }
}