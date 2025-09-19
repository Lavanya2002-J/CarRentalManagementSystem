using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CarRentalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(
            string searchString,
            int? seats,
            DateTime? pickupDate,
            DateTime? returnDate,
            decimal? maxRate)
        {
            // Start with cars that are globally available (not disabled or under maintenance)
            var carsQuery = _context.Cars.Where(c => c.IsAvailable == true);

            // --- 1. Date-wise Filtering (prevent showing cars already booked for selected dates) ---
            if (pickupDate.HasValue && returnDate.HasValue && returnDate > pickupDate)
            {
                carsQuery = carsQuery.Where(c =>
                    !_context.Bookings.Any(b =>
                        (b.Status == "Paid" || b.Status == "Pending") &&
                        b.CarID == c.CarID &&
                        b.PickupDate < returnDate.Value &&
                        b.ReturnDate > pickupDate.Value));
            }
            else if (pickupDate.HasValue && !returnDate.HasValue)
            {
                // Only pickup date provided → exclude cars still booked after pickupDate
                carsQuery = carsQuery.Where(c =>
                    !_context.Bookings.Any(b =>
                        (b.Status == "Paid" || b.Status == "Pending") &&
                        b.CarID == c.CarID &&
                        b.ReturnDate > pickupDate.Value));
            }
            else if (returnDate.HasValue && !pickupDate.HasValue)
            {
                // Only return date provided → exclude cars that start booking before returnDate
                carsQuery = carsQuery.Where(c =>
                    !_context.Bookings.Any(b =>
                        (b.Status == "Paid" || b.Status == "Pending") &&
                        b.CarID == c.CarID &&
                        b.PickupDate < returnDate.Value));
            }

            // --- 2. Car Name or Model Filtering (case-insensitive) ---
            if (!string.IsNullOrEmpty(searchString))
            {
                carsQuery = carsQuery.Where(c =>
                    EF.Functions.Like(c.CarName, $"%{searchString}%") ||
                    EF.Functions.Like(c.Model, $"%{searchString}%"));
            }

            // --- 3. Seats Filtering ---
            if (seats.HasValue && seats > 0)
            {
                carsQuery = carsQuery.Where(c => c.Seats == seats.Value);
            }

            // --- 4. Daily Rate Filtering ---
            if (maxRate.HasValue && maxRate > 0)
            {
                carsQuery = carsQuery.Where(c => c.DailyRate <= maxRate.Value);
            }

            // Execute the final query
            var filteredCars = await carsQuery.ToListAsync();

            // Check if user tried to filter anything
            var searchAttempted = !string.IsNullOrEmpty(searchString) ||
                                  seats.HasValue ||
                                  pickupDate.HasValue ||
                                  returnDate.HasValue ||
                                  maxRate.HasValue;

            // Pass filter values back to the view to preserve form inputs
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentSeats"] = seats;
            ViewData["CurrentPickupDate"] = pickupDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentReturnDate"] = returnDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentMaxRate"] = maxRate;
            ViewData["SearchAttempted"] = searchAttempted;

            return View(filteredCars);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
