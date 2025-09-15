using CarRentalManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action for the customer's dashboard
        public IActionResult Dashboard()
        {
            // Role-based access check
            if (HttpContext.Session.GetString("Role") != "Customer")
            {
                return RedirectToAction("CustomerLogin", "Account");
            }

            // Pass the username to the view using ViewBag
            ViewBag.Username = HttpContext.Session.GetString("Username");

            return View();
        }
    }
}