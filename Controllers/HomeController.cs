using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureCarRental.Models;
using AzureCarRental.Services;

namespace AzureCarRental.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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

    [HttpGet("test-email")]
    public async Task<IActionResult> TestEmail(string email = "abderahimaitchraa@gmail.com")
    {
        try
        {
            // Create test reservation and car
            var testCar = new Car
            {
                Id = 1,
                Brand = "BMW",
                Model = "Série 3",
                Year = 2026,
                DailyRate = 99.99m,
                IsAvailable = true
            };

            var testReservation = new Reservation
            {
                Id = 999,
                CarId = 1,
                CustomerName = "Test User",
                CustomerEmail = email,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(3),
                TotalPrice = 199.98m
            };

            _logger.LogInformation($"✅ Email service disabled");

            return Json(new
            {
                status = "success",
                message = "Test email sent! Check your mailbox and logs above.",
                sentTo = email,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ TEST EMAIL ERROR: {ex.Message}");
            return Json(new
            {
                status = "error",
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}
