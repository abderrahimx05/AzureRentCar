using Microsoft.AspNetCore.Mvc;
using AzureCarRental.Models;
using AzureCarRental.Data;
using AzureCarRental.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;

namespace AzureCarRental.Controllers;

public class CarsController : Controller
{
    private readonly Database _db;
    private readonly IImageService _images;

    public CarsController(Database db, IImageService images)
    {
        _db = db;
        _images = images;
    }

    public async Task<IActionResult> Index()
    {
        var cars = await _db.GetCarsAsync();
        return View(cars);
    }

    public async Task<IActionResult> Details(int id)
    {
        var car = await _db.GetCarAsync(id);
        if (car == null) return NotFound();
        return View(car);
    }

    [HttpGet]
    public async Task<IActionResult> Reserve(int id)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Auth");

        var car = await _db.GetCarAsync(id);
        if (car == null) return NotFound();
        ViewData["Car"] = car;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Reserve(Reservation res)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Auth");

        var car = await _db.GetCarAsync(res.CarId);
        if (car == null) return NotFound();

        var days = (res.EndDate - res.StartDate).Days;
        res.TotalPrice = days * car.DailyRate;

        var id = await _db.SaveReservationAsync(res);

        return RedirectToAction("Confirmation", new { id });
    }

    public IActionResult Confirmation(int id)
    {
        ViewData["ReservationId"] = id;
        return View();
    }

    // API endpoint to generate SAS URLs for Azure images on-the-fly
    [HttpGet("api/image-url")]
    public IActionResult GetImageUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest("URL is required");

        // If it's not an Azure blob, return it as-is
        if (!url.Contains("azurrentcar.blob.core.windows.net"))
            return Ok(new { url });

        try
        {
            // Extract blob name from URL
            var uri = new Uri(url);
            var path = uri.LocalPath;
            var blobName = path.Split('/').Last();

            if (string.IsNullOrEmpty(blobName))
                return Ok(new { url }); // Return original if extraction fails

            var connectionString = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("AzureStorage");

            if (string.IsNullOrEmpty(connectionString))
                return Ok(new { url }); // Fallback if no connection string

            // Extract account name and key
            string accountName = "";
            string accountKey = "";
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                var kvp = part.Split('=', 2);
                if (kvp.Length != 2) continue;

                var key = kvp[0].Trim();
                var value = kvp[1].Trim();

                if (key.Equals("AccountName", StringComparison.OrdinalIgnoreCase))
                    accountName = value;
                if (key.Equals("AccountKey", StringComparison.OrdinalIgnoreCase))
                    accountKey = value;
            }

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey))
                return Ok(new { url }); // Fallback if extraction fails

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = "images",
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(365)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            var sasUrl = $"{url}?{sasToken}";
            return Ok(new { url = sasUrl });
        }
        catch
        {
            return Ok(new { url }); // Fallback to original URL on error
        }
    }


}
