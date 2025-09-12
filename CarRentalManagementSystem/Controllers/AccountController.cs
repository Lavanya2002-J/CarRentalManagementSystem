using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- UNIFIED LOGIN ---
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins.FirstOrDefault(a => a.Username == model.Username && a.Password == model.Password);
                if (admin != null)
                {
                    HttpContext.Session.SetString("Username", admin.Username);
                    HttpContext.Session.SetString("Role", "Admin");
                    HttpContext.Session.SetString("UserID", admin.AdminID.ToString());
                    return RedirectToAction("Dashboard", "Admin");
                }

                var customer = _context.Customers.FirstOrDefault(c => c.Username == model.Username && c.Password == model.Password);
                if (customer != null)
                {
                    HttpContext.Session.SetString("Username", customer.Username);
                    HttpContext.Session.SetString("Role", "Customer");
                    HttpContext.Session.SetInt32("UserID", customer.CustomerID);
                    return RedirectToAction("Dashboard", "Customer");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }
            return View(model);
        }

        // --- CUSTOMER REGISTRATION ONLY ---
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Customer model)
        {
            if (ModelState.IsValid)
            {
                bool userExists = _context.Admins.Any(a => a.Username == model.Username) || _context.Customers.Any(c => c.Username == model.Username);
                if (userExists)
                {
                    ModelState.AddModelError("Username", "This username already exists.");
                    return View(model);
                }

                _context.Customers.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Registration completed successfully! You can now log in.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
