using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.ViewModels.ReportViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Role-based access check
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // --- 1. Monthly Revenue Report ---
            var revenueData = await _context.Payments
                .Where(p => p.PaymentStatus == "Completed")
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalRevenue = g.Sum(p => p.Amount)
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync(); // <-- This brings the results from the database into memory

            // Now that the data is in memory, we can format it using C# methods
            var monthlyRevenue = revenueData.Select(r => new MonthlyRevenueViewModel
            {
                MonthYear = new DateTime(r.Year, r.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                TotalRevenue = r.TotalRevenue
            }).ToList();


            // --- 2. Most Popular Cars Report (Top 10) ---
            var popularCars = await _context.Bookings
                .Where(b => b.Status == "Paid") // Counting confirmed bookings
                .GroupBy(b => new { b.Car.CarName, b.Car.Model })
                .Select(g => new PopularCarViewModel
                {
                    CarName = g.Key.CarName,
                    CarModel = g.Key.Model,
                    BookingCount = g.Count()
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(10)
                .ToListAsync();

            // --- 3. Best Customers Report (Top 10 by amount spent) ---
            var topCustomers = await _context.Bookings
                .Where(b => b.Status == "Paid") // Counting confirmed bookings
                .GroupBy(b => new { b.Customer.CustomerName, b.Customer.mail })
                .Select(g => new TopCustomerViewModel
                {
                    CustomerName = g.Key.CustomerName,
                    CustomerEmail = g.Key.mail,
                    TotalBookings = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalCost)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();


            var viewModel = new AdvancedReportViewModel
            {
                MonthlyRevenue = monthlyRevenue,
                PopularCars = popularCars,
                TopCustomers = topCustomers
            };

            return View(viewModel);
        }
    }
}
