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

        // ITHAI PUTHUSAAGA SERKAVUM
        // GET: Payment or /Payment/Index
        public async Task<IActionResult> Index()
        {
            // Anaithu payments-aiyum thethiyin adippadayil iranga varisaiyil edukkavum
            var payments = await _context.Payments
                                         .OrderByDescending(p => p.PaymentDate)
                                         .ToListAsync();
            return View(payments); // Payment list-ai view-ku anuppavum
        }

        // GET: Payment/Create
        [HttpGet]
        public async Task<IActionResult> Create(int bookingId, decimal amount)
        {
            // ... (intha method-la entha maatamum illai, appadiye irukkattum)
            var booking = await _context.Bookings
                                        .Include(b => b.Car)
                                        .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var viewModel = new PaymentViewModel
            {
                BookingID = booking.BookingID,
                Amount = booking.TotalCost,
                CarName = booking.Car.CarName,
                CarModel = booking.Car.CarModel,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                PaymentMethods = new List<SelectListItem>
                {
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
            // ... (intha method-la entha maatamum illai, appadiye irukkattum)
            if (ModelState.IsValid)
            {
                var booking = await _context.Bookings.FindAsync(viewModel.BookingID);
                if (booking == null) return NotFound();

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
                car.IsAvailable = "No";

                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { bookingId = booking.BookingID });
            }

            var bookingForDisplay = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.BookingID == viewModel.BookingID);
            viewModel.CarName = bookingForDisplay.Car.CarName;
            viewModel.CarModel = bookingForDisplay.Car.CarModel;
            viewModel.PickupDate = bookingForDisplay.PickupDate;
            viewModel.ReturnDate = bookingForDisplay.ReturnDate;
            viewModel.PaymentMethods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Cash on Pickup", Text = "Cash on Pickup (Manual)" },
                    new SelectListItem { Value = "Credit Card", Text = "Credit Card (Online)" }
                };
            return View(viewModel);
        }

        // GET: Payment/Success
        public async Task<IActionResult> Success(int bookingId)
        {
            // ... (intha method-la entha maatamum illai, appadiye irukkattum)
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
            // ... (intha method-la entha maatamum illai, appadiye irukkattum)
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