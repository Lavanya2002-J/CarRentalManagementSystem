using Microsoft.AspNetCore.Mvc;

namespace CarRentalManagementSystem.Controllers
{
    public class AdminController : Controller
    {

        public IActionResult Dashboard()
        {
            // Role-based access check
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }
            return View();
        }
    }
}
