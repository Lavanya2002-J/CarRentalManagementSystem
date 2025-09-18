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
            // Start with the base query for cars that are generally available.
            var carsQuery = _context.Cars.Where(c => c.IsAvailable == true);

            // --- 1. Date-wise Filtering ---
            // If a valid date range is provided, find and exclude cars that are already booked during that time.
            if (pickupDate.HasValue && returnDate.HasValue && returnDate > pickupDate)
            {
                // Find IDs of cars with bookings that overlap with the selected date range.
                var unavailableCarIds = await _context.Bookings
                    .Where(b => (b.Status == "Paid" || b.Status == "Pending") &&
                                b.PickupDate < returnDate.Value &&
                                b.ReturnDate > pickupDate.Value)
                    .Select(b => b.CarID)
                    .Distinct()
                    .ToListAsync();

                // Exclude these unavailable cars from our results.
                carsQuery = carsQuery.Where(c => !unavailableCarIds.Contains(c.CarID));
            }

            // --- 2. Car Name or Model Filtering ---
            if (!string.IsNullOrEmpty(searchString))
            {
                carsQuery = carsQuery.Where(c => c.CarName.Contains(searchString) || c.Model.Contains(searchString));
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

            // Execute the final query to get the list of cars.
            var filteredCars = await carsQuery.ToListAsync();

            // A flag to check if the user has tried to filter anything.
            var searchAttempted = !string.IsNullOrEmpty(searchString) ||
                                  seats.HasValue ||
                                  pickupDate.HasValue ||
                                  maxRate.HasValue;

            // Pass the current filter values back to the view to keep the form fields populated.
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