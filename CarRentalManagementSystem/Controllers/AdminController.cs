using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CarRentalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        // --- ADD THIS field and constructor to get database access ---
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- UPDATE THIS ENTIRE METHOD ---
        public async Task<IActionResult> Dashboard()
        {
            // Role-based access check
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // 1. Count of available cars
            var availableCars = await _context.Cars.CountAsync(c => c.IsAvailable == true);

            // 2. Count of active bookings
            var activeBookings = await _context.Bookings.CountAsync(b => b.Status == "Paid" || b.Status == "Pending");

            // 3. Revenue for the current month (September 2025)
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var nextMonth = startOfMonth.AddMonths(1);

            var monthlyRevenue = await _context.Payments
            .Where(p => p.PaymentDate >= startOfMonth && p.PaymentDate < nextMonth)
            .SumAsync(p => p.Amount);

            // 4. Total registered customers
            var totalCustomers = await _context.Customers.CountAsync();

            // Populate the ViewModel with the calculated stats
            var viewModel = new AdminDashboardViewModel
            {
                AvailableCarsCount = availableCars,
                ActiveBookingsCount = activeBookings,
                MonthlyRevenue = monthlyRevenue,
                TotalCustomersCount = totalCustomers
            };

            // Pass the ViewModel to the view
            return View(viewModel);
        }
    }
}
