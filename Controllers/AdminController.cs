using Microsoft.AspNetCore.Mvc;
using AzureCarRental.Models;
using AzureCarRental.Data;
using AzureCarRental.Services;

namespace AzureCarRental.Controllers;

public class AdminController : Controller
{
    private readonly Database _db;
    private readonly IImageService _images;

    public AdminController(Database db, IImageService images)
    {
        _db = db;
        _images = images;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
            return Unauthorized();

        var cars = await _db.GetCarsAsync();
        return View(cars);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAdmin())
            return Unauthorized();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Car car, IFormFile image)
    {
        if (!IsAdmin())
            return Unauthorized();

        if (image != null && image.Length > 0)
            car.ImageUrl = await _images.UploadAsync(image);

        await _db.SaveCarAsync(car);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAdmin())
            return Unauthorized();

        var car = await _db.GetCarAsync(id);
        if (car == null)
            return NotFound();
        return View(car);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Car car, IFormFile image)
    {
        if (!IsAdmin())
            return Unauthorized();

        if (image != null && image.Length > 0)
            car.ImageUrl = await _images.UploadAsync(image);

        await _db.UpdateCarAsync(car);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin())
            return Unauthorized();

        await _db.DeleteCarAsync(id);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Reservations()
    {
        if (!IsAdmin())
            return Unauthorized();

        var reservations = await _db.GetAllReservationsAsync();
        return View(reservations);
    }

    [HttpGet]
    public async Task<IActionResult> TestAzureStorage()
    {
        if (!IsAdmin())
            return Unauthorized();

        try
        {
            // Try to create a test blob
            var testContent = System.Text.Encoding.UTF8.GetBytes("Test file");
            var testFile = new FormFile(
                new MemoryStream(testContent), 0, testContent.Length,
                "file", "test.txt");

            var url = await _images.UploadAsync(testFile);

            if (string.IsNullOrEmpty(url))
                return Json(new { status = "error", message = "Upload returned empty URL" });

            return Json(new { status = "success", url = url });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestEmail()
    {
        if (!IsAdmin())
            return Unauthorized();

        try
        {
            // Create test reservation and car
            var testCar = new Car
            {
                Brand = "Test",
                Model = "Car",
                Year = 2026,
                DailyRate = 99.99m
            };

            var testReservation = new Reservation
            {
                Id = 999,
                CarId = 1,
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                TotalPrice = 199.98m
            };

            return Json(new
            {
                status = "success",
                message = "Email service has been disabled"
            });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", message = ex.Message });
        }
    }
}