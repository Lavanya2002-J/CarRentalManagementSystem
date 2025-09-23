using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalManagementSystem.Controllers
{
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper method to check if the current user is an Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Car
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            List<Car> cars = _context.Cars.ToList();
            return View(cars);
        }

        // GET: Car/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");


            return View(new CarViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CarViewModel viewModel)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            // --- Existing Duplicate Checks (Good) ---
            var registrationNumberExists = _context.Cars.Any(a => a.RegistrationNumber == viewModel.RegistrationNumber);
            if (registrationNumberExists)
            {
                ModelState.AddModelError("RegistrationNumber", "This Registration Number already exists.");
            }

            var insurancePolicyExists = _context.Cars.Any(a => a.InsurancePolicyNo == viewModel.InsurancePolicyNo);
            if (insurancePolicyExists)
            {
                ModelState.AddModelError("InsurancePolicyNo", "This Insurance Policy Number already exists.");
            }

            // --- NEW: Business Logic Validation ---
            if (viewModel.Seats <= 0)
            {
                ModelState.AddModelError("Seats", "Number of seats must be greater than zero.");
            }

            if (viewModel.DailyRate <= 0)
            {
                ModelState.AddModelError("DailyRate", "Daily Rate must be a positive number.");
            }

            if (viewModel.FuelCapacity <= 0)
            {
                ModelState.AddModelError("FuelCapacity", "Fuel Capacity must be greater than zero.");
            }

            if (viewModel.InsuranceExpiryDate.HasValue && viewModel.InsuranceExpiryDate.Value < DateTime.Today)
            {
                ModelState.AddModelError("InsuranceExpiryDate", "Insurance Expiry Date cannot be in the past.");
            }
            if (viewModel.LogoFile == null || viewModel.LogoFile.Length == 0)
            {
                ModelState.AddModelError("LogoFile", "Logo file is required.");
            }
            if (viewModel.CarImageFile == null || viewModel.CarImageFile.Length == 0)
            {
                ModelState.AddModelError("CarImageFile", "Car image file is required.");
            }

            // If any of the above custom validations failed, return the view immediately.
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // --- The rest of your code remains the same ---
            try
            {
                // The check is done above, but we keep it here as a final catch-all for data annotations.
                if (ModelState.IsValid)
                {
                    string logoFileName = null;
                    string imageFileName = null;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    if (viewModel.LogoFile != null)
                    {
                        logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.LogoFile.FileName);
                        string logoPath = Path.Combine(uploadsFolder, logoFileName);
                        using (var fs = new FileStream(logoPath, FileMode.Create)) { viewModel.LogoFile.CopyTo(fs); }
                    }

                    if (viewModel.CarImageFile != null)
                    {
                        imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.CarImageFile.FileName);
                        string imagePath = Path.Combine(uploadsFolder, imageFileName);
                        using (var fs = new FileStream(imagePath, FileMode.Create)) { viewModel.CarImageFile.CopyTo(fs); }
                    }

                    var car = new Car
                    {
                        CarID = Guid.NewGuid(),
                        CarName = viewModel.CarName,
                        Model = viewModel.Model,
                        FuelType = viewModel.FuelType,
                        Transmission = viewModel.Transmission,
                        Seats = viewModel.Seats,
                        DailyRate = viewModel.DailyRate,
                        IsAvailable = viewModel.IsAvailable,
                        LogoFileName = logoFileName,
                        CarImageFileName = imageFileName,
                        Description = viewModel.Description,
                        Color = viewModel.Color,
                        RegistrationNumber = viewModel.RegistrationNumber,
                        FuelCapacity = viewModel.FuelCapacity,
                        InsurancePolicyNo = viewModel.InsurancePolicyNo,
                        InsuranceExpiryDate = viewModel.InsuranceExpiryDate
                    };

                    _context.Cars.Add(car);
                    _context.SaveChanges();

                    TempData["Success"] = "Car added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(viewModel);
            }
        }

        

        // GET: Car/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();

            var viewModel = new CarViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Model = car.Model,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Seats = car.Seats,
                DailyRate = car.DailyRate,
                IsAvailable = car.IsAvailable,
                Description = car.Description,
                Color = car.Color,
                RegistrationNumber = car.RegistrationNumber,
                FuelCapacity = car.FuelCapacity,
                InsurancePolicyNo = car.InsurancePolicyNo,
                InsuranceExpiryDate = car.InsuranceExpiryDate,
                ExistingLogoPath = car.LogoFileName,
                ExistingCarImagePath = car.CarImageFileName
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Edit(CarViewModel viewModel)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            if (ModelState.IsValid)
            {
                var car = _context.Cars.FirstOrDefault(c => c.CarID == viewModel.CarID);
                if (car == null) return NotFound();

                car.CarName = viewModel.CarName;
                car.Model = viewModel.Model;
                car.FuelType = viewModel.FuelType;
                car.Transmission = viewModel.Transmission;
                car.Seats = viewModel.Seats;
                car.DailyRate = viewModel.DailyRate;
                car.IsAvailable = viewModel.IsAvailable;
                car.Description = viewModel.Description;
                car.Color = viewModel.Color;
                car.RegistrationNumber = viewModel.RegistrationNumber;
                car.FuelCapacity = viewModel.FuelCapacity;
                car.InsurancePolicyNo = viewModel.InsurancePolicyNo;
                car.InsuranceExpiryDate = viewModel.InsuranceExpiryDate;





                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                if (viewModel.LogoFile != null)
                {
                    if (!string.IsNullOrEmpty(car.LogoFileName))
                    {
                        var oldLogoPath = Path.Combine(uploadsFolder, car.LogoFileName);
                        if (System.IO.File.Exists(oldLogoPath)) { System.IO.File.Delete(oldLogoPath); }
                    }
                    car.LogoFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.LogoFile.FileName);
                    var logoPath = Path.Combine(uploadsFolder, car.LogoFileName);
                    using (var fs = new FileStream(logoPath, FileMode.Create)) { viewModel.LogoFile.CopyTo(fs); }
                }

                if (viewModel.CarImageFile != null)
                {
                    if (!string.IsNullOrEmpty(car.CarImageFileName))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, car.CarImageFileName);
                        if (System.IO.File.Exists(oldImagePath)) { System.IO.File.Delete(oldImagePath); }
                    }
                    car.CarImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.CarImageFile.FileName);
                    var imagePath = Path.Combine(uploadsFolder, car.CarImageFileName);
                    using (var fs = new FileStream(imagePath, FileMode.Create)) { viewModel.CarImageFile.CopyTo(fs); }
                }

                _context.Cars.Update(car);
                _context.SaveChanges();

                TempData["Success"] = "Car updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Car/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();
            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!string.IsNullOrEmpty(car.LogoFileName))
            {
                string logoPath = Path.Combine(uploadsFolder, car.LogoFileName);
                if (System.IO.File.Exists(logoPath)) System.IO.File.Delete(logoPath);
            }
            if (!string.IsNullOrEmpty(car.CarImageFileName))
            {
                string imagePath = Path.Combine(uploadsFolder, car.CarImageFileName);
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _context.Cars.Remove(car);
            _context.SaveChanges();

            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Car/Details/{id}
        public IActionResult Details(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarID == id);
            if (car == null) return NotFound();

            var viewModel = new CarViewModel
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Model = car.Model,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Seats = car.Seats,
                DailyRate = car.DailyRate,
                IsAvailable = car.IsAvailable,
                Description = car.Description,
                Color = car.Color,
                RegistrationNumber = car.RegistrationNumber,
                FuelCapacity = car.FuelCapacity,
                InsurancePolicyNo = car.InsurancePolicyNo,
                InsuranceExpiryDate = car.InsuranceExpiryDate,
                ExistingLogoPath = car.LogoFileName,
                ExistingCarImagePath = car.CarImageFileName
            };
            return View(viewModel);
        }

    }
}