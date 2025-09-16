using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
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
        // --- CUSTOMER MANAGEMENT ---

        // GET: /Admin/ManageCustomers (List all customers)
        public IActionResult ManageCustomers()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }
            var customers = _context.Customers.OrderBy(c => c.CustomerName).ToList();
            return View(customers);
        }

        // GET: /Admin/AddCustomer (Show form to add a new customer)
        public IActionResult AddCustomer()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }
            return View(new RegisterViewModel());
        }

        // POST: /Admin/AddCustomer (Save the new customer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCustomer(RegisterViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            if (ModelState.IsValid)
            {
                bool userExists = _context.Customers.Any(c => c.Username == model.Username || c.Email == model.Email);
                if (userExists)
                {
                    ModelState.AddModelError("", "A customer with this username or email already exists.");
                    return View(model);
                }

                var customer = new Customer
                {
                    Username = model.Username,
                    Password = model.Password,
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    NIC = model.NIC,
                    LicenseNo = model.LicenseNo
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Customer '{customer.CustomerName}' was added successfully!";
                return RedirectToAction(nameof(ManageCustomers));
            }
            return View(model);
        }

        // GET: /Admin/EditCustomer/5 (Show form to edit a customer)
        public IActionResult EditCustomer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: /Admin/EditCustomer/5 (Save changes for a customer)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return NotFound();
            }

            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            if (ModelState.IsValid)
            {
                // Optional: Check for username uniqueness if it was changed
                bool usernameExists = _context.Customers.Any(c => c.Username == customer.Username && c.CustomerID != id);
                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "This username is already taken by another user.");
                    return View(customer);
                }

                _context.Update(customer);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Customer '{customer.CustomerName}' was updated successfully!";
                return RedirectToAction(nameof(ManageCustomers));
            }
            return View(customer);
        }

        // GET: /Admin/DeleteCustomer/5 (Show delete confirmation page)
        public IActionResult DeleteCustomer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var customer = _context.Customers.FirstOrDefault(m => m.CustomerID == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: /Admin/DeleteCustomer/5 (Confirm and delete the customer)
        [HttpPost, ActionName("DeleteCustomer")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCustomerConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Customer '{customer.CustomerName}' was deleted successfully.";
            }
            return RedirectToAction(nameof(ManageCustomers));
        }
    }









}

