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

        public IActionResult AdminLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins.FirstOrDefault(a => (a.Username == model.UsernameOrEmail || a.Email == model.UsernameOrEmail) && a.Password == model.Password);
                if (admin != null)
                {
                    HttpContext.Session.SetString("Username", admin.Username);
                    HttpContext.Session.SetString("Role", "Admin");
                    HttpContext.Session.SetString("UserID", admin.AdminID.ToString());
                    return RedirectToAction("Dashboard", "Admin");
                }
                ModelState.AddModelError("", "Invalid admin username / Email or password.");
            }
            return View(model);
        }

        public IActionResult CustomerLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _context.Customers.FirstOrDefault(c => (c.Username == model.UsernameOrEmail || c.Email == model.UsernameOrEmail) && c.Password == model.Password);
                if (customer != null)
                {
                    HttpContext.Session.SetString("Username", customer.Username);
                    HttpContext.Session.SetString("Role", "Customer");
                    HttpContext.Session.SetInt32("UserID", customer.CustomerID);
                    return RedirectToAction("Dashboard", "Customer");
                }
                ModelState.AddModelError("", "Invalid customer username / Email  or password.");
            }
            return View(model);
        }

        // --- UPDATED CUSTOMER REGISTRATION ---

        // GET: /Account/Register
        public IActionResult Register()
        {
            // Pass a new instance of the ViewModel to the view
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists in Admins or Customers tables
                bool userExists = _context.Admins.Any(a => a.Username == model.Username) || _context.Customers.Any(c => c.Username == model.Username);
                if (userExists)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose another.");
                    return View(model);
                }
                bool Emailvalid = _context.Admins.Any(a => a.Email == model.Email) || _context.Customers.Any(a => a.Email == model.Email);
                if (Emailvalid)
                {
                    ModelState.AddModelError("Email", "This email already exists.");
                    return View(model);
                }
                bool NICvalid = _context.Customers.Any(a => a.NIC == model.NIC);
                if (NICvalid)
                {
                    ModelState.AddModelError("NIC", "This NIC already exists.");
                    return View(model);
                }

                bool Licvalid = _context.Customers.Any(a => a.LicenseNo == model.LicenseNo);
                if (Licvalid)
                {
                    ModelState.AddModelError("LicenseNo", "This License Number already exists.");
                    return View(model);
                }
                // Map data from ViewModel to the Customer model
                var customer = new Customer
                {
                    Username = model.Username,
                    Password = model.Password, // IMPORTANT: In a real app, you must hash the password here!
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    NIC = model.NIC,
                    LicenseNo = model.LicenseNo
                };

                // Add the new customer to the database
                _context.Customers.Add(customer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration completed successfully! You can now log in.";
                return RedirectToAction("CustomerLogin");
            }

            // If model state is not valid, return to the view with the entered data
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Add these two actions inside your existing AccountController class

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            // 1. Get the logged-in customer's ID from the session
            var customerId = HttpContext.Session.GetInt32("UserID");

            // 2. Ensure the user is logged in
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            // 3. Find the customer in the database
            var customer = _context.Customers.Find(customerId.Value);
            if (customer == null)
            {
                return NotFound(); // Or handle as an error
            }

            // 4. Map the customer data to the ProfileViewModel
            var model = new ProfileViewModel
            {
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                NIC = customer.NIC,
                LicenseNo = customer.LicenseNo
            };

            // 5. Return the view with the populated model
            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Get the logged-in customer's ID from the session
            var customerId = HttpContext.Session.GetInt32("UserID");
            if (customerId == null)
            {
                return Unauthorized();
            }

            // 2. Get the customer entity from the database to update it
            var customerToUpdate = _context.Customers.Find(customerId.Value);
            if (customerToUpdate == null)
            {
                return NotFound();
            }

            // 3. Check if the username has been changed and if the new one is unique
            if (customerToUpdate.Username != model.Username)
            {
                bool usernameExists = _context.Customers.Any(c => c.Username == model.Username && c.CustomerID != customerId.Value);
                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose another.");
                    return View(model);
                }
                customerToUpdate.Username = model.Username;
            }

            // 4. Check if the user wants to change their password
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // 4a. Verify their old password is correct
                if (customerToUpdate.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "The current password you entered is incorrect.");
                    return View(model);
                }
                // 4b. Update to the new password (in a real app, HASH THIS PASSWORD!)
                customerToUpdate.Password = model.NewPassword;
            }

            // 5. Update the rest of the customer's information
            customerToUpdate.CustomerName = model.CustomerName;
            customerToUpdate.Email = model.Email;
            customerToUpdate.PhoneNumber = model.PhoneNumber;
            customerToUpdate.Address = model.Address;
            customerToUpdate.NIC = model.NIC;
            customerToUpdate.LicenseNo = model.LicenseNo;

            // 6. Save the changes to the database
            _context.SaveChanges();

            // 7. Set a success message and redirect
            TempData["SuccessMessage"] = "Your profile has been updated successfully!";
            return RedirectToAction("Profile");
        }

    }
}
