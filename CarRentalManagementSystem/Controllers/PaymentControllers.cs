//using CarRentalManagementSystem.Data;
//using CarRentalManagementSystem.Models;
//using CarRentalManagementSystem.ViewModels; // ✅ Make sure to include this
//using Microsoft.AspNetCore.Mvc;

//namespace CarRentalManagementSystem.Controllers
//{
//    public class PaymentController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public PaymentController(ApplicationDbContext context)
//        {
//            _context = context;
//        }
//        // GET: Payment/Create
//        public IActionResult Create(int bookingId, decimal amount)
//        {
//            var viewModel = new PaymentViewModel
//            {
//                BookingID = bookingId,
//                Amount = amount
//            };
//            return View(viewModel);
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(PaymentViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Simulate payment
//                string transactionId = Guid.NewGuid().ToString().Substring(0, 10);

//                var payment = new Payment
//                {
//                    BookingID = model.BookingID,
//                    Amount = model.Amount,
//                    PaymentDate = DateTime.Now,
//                    PaymentMethod = model.PaymentMethod,
//                    PaymentStatus = "Success",
//                    TransactionID = transactionId
//                };

//                _context.Payments.Add(payment);

//                // Update Booking status to Paid
//                var booking = _context.Bookings.Find(model.BookingID);
//                if (booking != null)
//                {
//                    booking.Status = "Paid";
//                    _context.Bookings.Update(booking);
//                }
//                var car = _context.Cars.Find(booking.CarID);
//                if (car != null)
//                {
//                    car.IsAvailable = "No";
//                    _context.Cars.Update(car);
//                }

//                _context.SaveChanges();

//                return RedirectToAction("Confirmation", new { id = payment.PaymentID });
//            }

//            return View(model);
//        }


//        public IActionResult Confirmation(int id)
//        {
//            var payment = _context.Payments.Find(id);
//            if (payment == null) return NotFound();
//            return View(payment);
//        }

//        public IActionResult Index()
//        {
//            var payments = _context.Payments;
//            return View(payments);
//        }
//    }
//}
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalManagementSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payment/Create
        public IActionResult Create(int bookingId, decimal amount)
        {
            var viewModel = new PaymentViewModel
            {
                BookingID = bookingId,
                Amount = amount
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Simulate payment
                string transactionId = Guid.NewGuid().ToString().Substring(0, 10);

                var payment = new Payment
                {
                    BookingID = model.BookingID,
                    Amount = model.Amount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = "Success",
                    TransactionID = transactionId
                };

                _context.Payments.Add(payment);

                // Find the booking
                var booking = _context.Bookings.Find(model.BookingID);

                // ✅ FIXED LOGIC: Only proceed if the booking exists
                if (booking != null)
                {
                    // 1. Update Booking status to Paid
                    booking.Status = "Paid";
                    _context.Bookings.Update(booking);

                    // 2. Find the associated car and make it unavailable
                    var car = _context.Cars.Find(booking.CarID);
                    if (car != null)
                    {
                        car.IsAvailable = "No";
                        _context.Cars.Update(car);
                    }
                }

                _context.SaveChanges();

                return RedirectToAction("Confirmation", new { id = payment.PaymentID });
            }

            return View(model);
        }

        public IActionResult Confirmation(int id)
        {
            var payment = _context.Payments.Find(id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        public IActionResult Index()
        {
            var payments = _context.Payments.ToList(); // Added .ToList() for immediate execution
            return View(payments);
        }
    }
}