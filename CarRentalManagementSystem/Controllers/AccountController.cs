using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace CarRentalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GmailSettings _gmailSettings;

        public AccountController(ApplicationDbContext context, IOptions<GmailSettings> gmailOptions)
        {
            _context = context;
            _gmailSettings = gmailOptions.Value; 
        }

        public IActionResult AdminLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins.FirstOrDefault(a => (a.Username == model.UsernameOrEmail || a.mail == model.UsernameOrEmail) && a.Password == model.Password);
                if (admin != null)
                {
                    HttpContext.Session.SetString("Username", admin.Username);
                    HttpContext.Session.SetString("Role", "Admin");
                    HttpContext.Session.SetString("UserID", admin.AdminID.ToString());
                    return RedirectToAction("Dashboard", "Admin");
                }

                ModelState.AddModelError("", "Invalid admin username/Email or password.");

            }
            return View(model);
        }

        
        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            
            bool userExists = _context.Admins.Any(a => a.Username == vm.Username) || _context.Customers.Any(c => c.Username == vm.Username);
            if (userExists)
            {
                ModelState.AddModelError("Username", "This username is already taken. Please choose another.");
                
            }

            bool Emailvalid = _context.Admins.Any(a => a.mail == vm.Email) || _context.Customers.Any(c => c.mail == vm.Email);


            if (Emailvalid)
            {
                ModelState.AddModelError("Email", "This email already exists.");
               
            }
            bool NICvalid = _context.Customers.Any(a => a.NIC == vm.NIC);
            if (NICvalid)
            {
                ModelState.AddModelError("NIC", "This NIC already exists.");
               
            }

            bool Licvalid = _context.Customers.Any(a => a.LicenseNo == vm.LicenseNo);
            if (Licvalid)
            {
                ModelState.AddModelError("LicenseNo", "This License Number already exists.");
            
            }
            if (!ModelState.IsValid) return View(vm);



            var customer = new Customer
            {
                Username = vm.Username,
                Password = vm.Password, // IMPORTANT: In a real app, you must hash the password here!
                CustomerName = vm.CustomerName,
                mail = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                NIC = vm.NIC,
                LicenseNo = vm.LicenseNo,
                IsVerified = false,
                VerificationToken = GenerateToken(),
                VerificationTokenExpires = DateTime.UtcNow.AddHours(24)


            };

          

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

           
            var verifyLink = Url.Action("VerifyEmail", "Account",
                new { token = customer.VerificationToken }, Request.Scheme);

            var html = $"Hi {customer.CustomerName},<br/>Click <a href='{verifyLink}'>here</a> to verify your email. Link expires in 24 hours.";
            await SendEmailAsync(customer.mail, "Verify your email", html);

            TempData["ShowPopup"] = "Check your email to verify your account!";
            return RedirectToAction("CustomerLogin");
        }

        [HttpGet]
        public IActionResult VerifySent() => View();

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("Invalid token.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c =>
                c.VerificationToken == token &&
                c.VerificationTokenExpires > DateTime.UtcNow);

            if (customer == null) return Content("Token invalid or expired.");

            customer.IsVerified = true;
            customer.VerificationToken = null;
            customer.VerificationTokenExpires = null;

            _context.Update(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("CustomerLogin");


        

        }



        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.mail == email);

            if (customer != null)
            {
               
                customer.PasswordResetToken = GenerateToken();
                customer.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
                _context.Update(customer);
                await _context.SaveChangesAsync();

                var link = Url.Action("ResetPassword", "Account", new { token = customer.PasswordResetToken }, Request.Scheme);
                var html = $"Hi {customer.CustomerName},<br/>Click <a href='{link}'>here</a> to reset your password. Link expires in 1 hour.";
                await SendEmailAsync(customer.mail, "Password Reset", html);
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();
 
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return Content("Invalid token.");
            var vm = new ResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var customer = await _context.Customers.FirstOrDefaultAsync(c =>
                c.PasswordResetToken == vm.Token &&
                c.PasswordResetTokenExpires > DateTime.UtcNow);

            if (customer == null)
            {
                ModelState.AddModelError("", "Token invalid or expired.");
                return View(vm);
            }

            customer.Password = vm.NewPassword;
            customer.PasswordResetToken = null;
            customer.PasswordResetTokenExpires = null;

            _context.Update(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("CustomerLogin");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        // ================= HELPERS =================
        private static string GenerateToken(int bytesLength = 32)
        {
            var bytes = new byte[bytesLength];
            RandomNumberGenerator.Fill(bytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_gmailSettings.Email, _gmailSettings.AppPassword),
                EnableSsl = true
            };

            var msg = new MailMessage
            {
                From = new MailAddress(_gmailSettings.Email, "Car Rental System"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);

            await smtp.SendMailAsync(msg);
        }
        public IActionResult CustomerLogin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerLogin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _context.Customers.FirstOrDefault(c => (c.Username == model.UsernameOrEmail || c.mail == model.UsernameOrEmail) && (c.Password == model.Password) && (c.IsVerified==true || c.IsVerified==false && c.VerificationTokenExpires > DateTime.Now));
                
                if (customer != null)
                {
                    HttpContext.Session.SetString("Username", customer.Username);
                    HttpContext.Session.SetString("Role", "Customer");
                    HttpContext.Session.SetInt32("UserID", customer.CustomerID);
                    return RedirectToAction("Dashboard", "Customer");
                }

                ModelState.AddModelError("", "Invalid customer username/Email or password.");


            }
            
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
                Email = customer.mail,
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
            customerToUpdate.mail = model.Email;
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
//HUHU