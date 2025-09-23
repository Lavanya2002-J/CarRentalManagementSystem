using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalManagementSystem.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Feedback (Admin view)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // 1. Fetch all feedback WITHOUT the .Include() calls
            var allFeedback = await _context.Feedbacks
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            // 2. Load all Customers and Cars into ViewBag dictionaries for quick lookups in the view
            ViewBag.Customers = await _context.Customers.ToDictionaryAsync(c => c.CustomerID, c => c.CustomerName);
            ViewBag.Cars = await _context.Cars.ToDictionaryAsync(c => c.CarID, c => $"{c.CarName} ({c.Model})");

            return View(allFeedback);
        }

        // GET: Feedback/Create/5 (for a specific booking)
        public async Task<IActionResult> Create(int bookingId)
        {
            if (HttpContext.Session.GetString("Role") != "Customer")
            {
                TempData["ErrorMessage"] = "You must be logged in to leave feedback.";
                return RedirectToAction("CustomerLogin", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("UserID");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.CustomerID == customerId.Value);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("History", "Booking");
            }

         

            var viewModel = new FeedbackViewModel
            {
                BookingID = booking.BookingID,
                CarID = booking.CarID,
                CustomerID = booking.CustomerID,
                CarName = booking.Car.CarName,
                CarModel = booking.Car.Model
            };

            return View(viewModel);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackViewModel viewModel)
        {
            if (HttpContext.Session.GetString("Role") != "Customer")
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            if (ModelState.IsValid)
            {
                var feedback = new Feedback
                {
                    CustomerID = viewModel.CustomerID,
                    CarID = viewModel.CarID,
                    Ratings = viewModel.Ratings,
                    FeedBack = viewModel.FeedBack
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you for your feedback!";
                return RedirectToAction("History", "Booking");
            }

            // If model is invalid, repopulate car details and return to view
            var car = await _context.Cars.FindAsync(viewModel.CarID);
            viewModel.CarName = car.CarName;
            viewModel.CarModel = car.Model;

            return View(viewModel);
        }
    }
}
