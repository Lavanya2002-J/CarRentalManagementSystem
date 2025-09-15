using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public IActionResult Create(CarViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");



            try
            {
                if (ModelState.IsValid)
                {
                    string logoFileName = null;
                    string imageFileName = null;

                    // ✅ Step 1: Get the path to the "uploads" folder
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    // ✅ Step 2: Create the folder if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // ✅ Step 3: Save the logo file
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                        string logoPath = Path.Combine(uploadsFolder, logoFileName);

                        using (var fs = new FileStream(logoPath, FileMode.Create))
                        {
                            model.LogoFile.CopyTo(fs);
                        }
                    }

                    // ✅ Step 4: Save the car image file
                    if (model.CarImageFile != null && model.CarImageFile.Length > 0)
                    {
                        imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CarImageFile.FileName);
                        string imagePath = Path.Combine(uploadsFolder, imageFileName);

                        using (var fs = new FileStream(imagePath, FileMode.Create))
                        {
                            model.CarImageFile.CopyTo(fs);
                        }
                    }

                    // ✅ Step 5: Map and save the car entity
                    var car = new Car
                    {
                        CarId = Guid.NewGuid(),
                        CarName = model.CarName,
                        CarModel = model.CarModel,
                        FuelType = model.FuelType,
                        Transmission = model.Transmission,
                        Seats = model.Seats,
                        DailyRate = model.DailyRate,
                        IsAvailable = model.IsAvailable,
                        Branch = model.Branch,
                        LogoFileName = logoFileName,
                        CarImageFileName = imageFileName,
                        Description = model.Description,
                        Color = model.Color,
                        RegistrationNumber = model.RegistrationNumber
                    };

                    _context.Cars.Add(car);
                    _context.SaveChanges();

                    TempData["Success"] = "Car added successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // if ModelState is NOT valid, return the view with model
                return View(model);
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                ModelState.AddModelError("", "An error occurred while saving the car: " + ex.Message);

                //  return the view with the current model
                return View(model);
            }
        }

        // GET: Car/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");


            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);
            if (car == null) return NotFound();

            // Map entity to ViewModel
            var model = new CarViewModel
            {
                CarId = car.CarId,
                CarName = car.CarName,
                CarModel = car.CarModel,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Seats = car.Seats,
                DailyRate = car.DailyRate,
                IsAvailable = car.IsAvailable,
                Branch = car.Branch,
                Description = car.Description,
                Color = car.Color,
                RegistrationNumber = car.RegistrationNumber
                // Existing file paths for display
                ,
                ExistingLogoPath = car.LogoFileName,
                ExistingCarImagePath = car.CarImageFileName

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CarViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            if (ModelState.IsValid)
            {
                var car = _context.Cars.FirstOrDefault(c => c.CarId == model.CarId);
                if (car == null) return NotFound();

                car.CarName = model.CarName;
                car.CarModel = model.CarModel;
                car.FuelType = model.FuelType;
                car.Transmission = model.Transmission;
                car.Seats = model.Seats;
                car.DailyRate = model.DailyRate;
                car.IsAvailable = model.IsAvailable;
                car.Branch = model.Branch;
                car.Description = model.Description;
                car.Color = model.Color;
                car.RegistrationNumber = model.RegistrationNumber;

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                // Handle logo file upload
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    // Delete old logo if exists (optional)
                    if (!string.IsNullOrEmpty(car.LogoFileName))
                    {
                        var oldLogoPath = Path.Combine(uploadsFolder, car.LogoFileName);
                        if (System.IO.File.Exists(oldLogoPath))
                        {
                            System.IO.File.Delete(oldLogoPath);
                        }
                    }

                    string logoFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.LogoFile.FileName);
                    string logoPath = Path.Combine(uploadsFolder, logoFileName);

                    using (var fs = new FileStream(logoPath, FileMode.Create))
                    {
                        model.LogoFile.CopyTo(fs);
                    }

                    car.LogoFileName = logoFileName;
                }
                else
                {
                    // Keep existing logo if no new file uploaded
                    car.LogoFileName = model.ExistingLogoPath;
                }

                // Handle car image file upload
                if (model.CarImageFile != null && model.CarImageFile.Length > 0)
                {
                    // Delete old image if exists (optional)
                    if (!string.IsNullOrEmpty(car.CarImageFileName))
                    {
                        var oldImagePath = Path.Combine(uploadsFolder, car.CarImageFileName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CarImageFile.FileName);
                    string imagePath = Path.Combine(uploadsFolder, imageFileName);

                    using (var fs = new FileStream(imagePath, FileMode.Create))
                    {
                        model.CarImageFile.CopyTo(fs);
                    }

                    car.CarImageFileName = imageFileName;
                }
                else
                {
                    // Keep existing image if no new file uploaded
                    car.CarImageFileName = model.ExistingCarImagePath;
                }

                _context.Cars.Update(car);
                _context.SaveChanges();

                TempData["Success"] = "Car updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        // GET: Car/Delete/{id}
        public IActionResult Delete(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);
            if (car == null) return NotFound();

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);
            if (car == null) return NotFound();

            // Delete image files
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!string.IsNullOrEmpty(car.LogoFileName))
            {
                string logoPath = Path.Combine(uploadsFolder, car.LogoFileName);
                if (System.IO.File.Exists(logoPath))
                    System.IO.File.Delete(logoPath);
            }

            if (!string.IsNullOrEmpty(car.CarImageFileName))
            {
                string imagePath = Path.Combine(uploadsFolder, car.CarImageFileName);
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Cars.Remove(car);
            _context.SaveChanges();

            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");


            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);
            if (car == null) return NotFound();

            // Map to ViewModel
            var model = new CarViewModel
            {
                CarId = car.CarId,
                CarName = car.CarName,
                CarModel = car.CarModel,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Seats = car.Seats,
                DailyRate = car.DailyRate,
                IsAvailable = car.IsAvailable,
                Branch = car.Branch,
                Description = car.Description,
                Color = car.Color,
                RegistrationNumber = car.RegistrationNumber,
                ExistingLogoPath = car.LogoFileName,
                ExistingCarImagePath = car.CarImageFileName
            };

            return View(model);
        }

    }
}