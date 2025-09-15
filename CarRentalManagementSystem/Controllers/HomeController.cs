using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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


        public async Task<IActionResult> Index(string searchString, string fuelType, string transmission, int? seats)
        {
            var carsQuery = _context.Cars.Where(c => c.IsAvailable == true);

            //  A flag to check if any filter is active
            var searchAttempted = !string.IsNullOrEmpty(searchString) ||
                                  !string.IsNullOrEmpty(fuelType) ||
                                  !string.IsNullOrEmpty(transmission) ||
                                  seats.HasValue;

            if (!string.IsNullOrEmpty(searchString))
            {
                carsQuery = carsQuery.Where(c => c.CarName.Contains(searchString) || c.Model.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(fuelType))
            {
                carsQuery = carsQuery.Where(c => c.FuelType == fuelType);
            }

            if (!string.IsNullOrEmpty(transmission))
            {
                carsQuery = carsQuery.Where(c => c.Transmission == transmission);
            }

            if (seats.HasValue)
            {
                carsQuery = carsQuery.Where(c => c.Seats == seats.Value);
            }

            var filteredCars = await carsQuery.ToListAsync();

            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentFuelType"] = fuelType;
            ViewData["CurrentTransmission"] = transmission;
            ViewData["CurrentSeats"] = seats;

            //  Pass the flag to the view
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
