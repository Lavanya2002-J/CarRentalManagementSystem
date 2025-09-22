using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CarRentalManagementSystem.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly GmailSettings _gmailSettings;

        public EmailController(ApplicationDbContext db, IOptions<GmailSettings> gmailOptions)
        {
            _db = db;
            _gmailSettings = gmailOptions.Value;
        }

        // List all rentals
        public async Task<IActionResult> Index()
        {

            var bookings = await _db.Bookings
                .Include(b => b.Customer)   // load related customer
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> ViewAllBookings()
        {
            // Role-based access check
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // Fetch all bookings from the database
            // Use Include() to load related Customer and Car data to avoid extra queries
            var allBookings = await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Car)
                .OrderByDescending(b => b.BookingID) // Show the latest bookings first
                .ToListAsync();

            return View(allBookings);
        }

        public async Task<IActionResult> SendReminders()
        {
            var today = DateTime.Today;

            var dueBookings = await _db.Bookings
                .Include(b => b.Customer) // Customer data load
                .Where(b => !b.ReturnEmailSent && b.ReturnDate.Date == today)
                .ToListAsync();

            foreach (var booking in dueBookings)
            {
                try
                {
                    SendEmail(
                        booking.Customer.mail,   // from Customer model
                        "Car Return Reminder",
                        $"Hello {booking.Customer.CustomerName},\nYour car is due for return today ({today:yyyy-MM-dd}\n  )."
                    );

                    booking.ReturnEmailSent = true;
                    _db.Update(booking);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending email: " + ex.Message);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SendPickup()
        {
            var today = DateTime.Today;

            var dueBookings = await _db.Bookings
                .Include(b => b.Customer) // Customer data load
                .Where(b => !b.PickupEmailSent && b.PickupDate.Date == today)
                .ToListAsync();

            foreach (var booking in dueBookings)
            {
                try
                {
                    SendEmail(
                        booking.Customer.mail,   // from Customer model
                        "Ethu car book panaika ",
                        $"Hello {booking.Customer.CustomerName},\n Thankyou for booking ({today:yyyy-MM-dd}   )."
                    );

                    booking.PickupEmailSent = true;
                    _db.Update(booking);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending email: " + ex.Message);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Simple email sender
        private void SendEmail(string toEmail, string subject, string body)
        {


            try
            {
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_gmailSettings.Email, _gmailSettings.AppPassword),
                    EnableSsl = true,
                };

                var message = new MailMessage(_gmailSettings.Email, toEmail, subject, body);
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // log error
                Console.WriteLine("Email send failed: " + ex.Message);
            }
        }
    }
}

