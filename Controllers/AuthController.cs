using Microsoft.AspNetCore.Mvc;
using AzureCarRental.Models;
using AzureCarRental.Data;
using AzureCarRental.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace AzureCarRental.Controllers;

public class AuthController : Controller
{
    private readonly Database _db;
    private readonly IAuthService _auth;
    private readonly IAzureAuthService _azureAuth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(Database db, IAuthService auth, IAzureAuthService azureAuth, ILogger<AuthController> logger)
    {
        _db = db;
        _auth = auth;
        _azureAuth = azureAuth;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var users = await _db.GetAllUsersAsync();
        var user = users.FirstOrDefault(u => u.Email == email && u.Password == password);

        if (user == null)
        {
            ViewData["Error"] = "Invalid email or password";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);

        if (user.Role == "Admin")
            return RedirectToAction("Index", "Admin");
        else
            return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewData["Error"] = "Passwords do not match";
            return View();
        }

        var users = await _db.GetAllUsersAsync();
        if (users.Any(u => u.Email == email))
        {
            ViewData["Error"] = "Email already registered";
            return View();
        }

        var user = new User { Name = name, Email = email, Password = password, Role = "User", CreatedAt = DateTime.Now };
        await _db.SaveUserAsync(user);

        ViewData["Success"] = "Registration successful! Please log in.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login");

        var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
        var reservations = await _db.GetUserReservationsAsync(userEmail);
        return View(reservations);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Cars");
    }

    [HttpGet]
    public IActionResult LoginWithAzure()
    {
        var redirectUrl = Url.Action("AzureCallback", "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> AzureCallback()
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("Authentification Azure AD échouée");
                return RedirectToAction("Login");
            }

            var claimsPrincipal = authenticateResult.Principal;
            var email = claimsPrincipal?.FindFirstValue(ClaimTypes.Email) ??
                       claimsPrincipal?.FindFirstValue("preferred_username") ?? "";
            var name = claimsPrincipal?.FindFirstValue(ClaimTypes.Name) ?? "Azure User";

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email non trouvé dans les claims Azure AD");
                ViewData["Error"] = "Unable to retrieve email from Azure AD";
                return View("Login");
            }

            _logger.LogInformation($"Utilisateur Azure AD connecté: {email}");

            // Obtenir ou créer l'utilisateur
            var user = await _azureAuth.GetOrCreateUserFromAzureAsync(email, name);

            if (user == null)
            {
                _logger.LogError($"Impossible de créer/récupérer l'utilisateur: {email}");
                ViewData["Error"] = "Failed to create user account";
                return View("Login");
            }

            // Créer la session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("AuthProvider", "Azure");

            _logger.LogInformation($"Session créée pour: {user.Email}");

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur lors du callback Azure: {ex.Message}");
            ViewData["Error"] = "An error occurred during Azure authentication";
            return View("Login");
        }
    }

    [HttpPost]
    public async Task<IActionResult> LogoutWithAzure()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Cars");
    }
}
