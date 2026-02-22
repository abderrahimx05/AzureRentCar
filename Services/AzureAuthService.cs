using AzureCarRental.Models;
using AzureCarRental.Data;

namespace AzureCarRental.Services;

public interface IAzureAuthService
{
    Task<User?> GetOrCreateUserFromAzureAsync(string email, string name);
    Task<bool> IsUserInRoleAsync(string userId, string role);
}

public class AzureAuthService : IAzureAuthService
{
    private readonly Database _db;
    private readonly ILogger<AzureAuthService> _logger;

    public AzureAuthService(Database db, ILogger<AzureAuthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Récupère ou crée un utilisateur basé sur les informations Azure AD
    /// </summary>
    public async Task<User?> GetOrCreateUserFromAzureAsync(string email, string name)
    {
        try
        {
            var users = await _db.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                _logger.LogInformation($"Création d'un nouvel utilisateur Azure AD: {email}");

                user = new User
                {
                    Name = name,
                    Email = email,
                    Password = Guid.NewGuid().ToString(), // Génère un mot de passe temporaire
                    Role = "User",
                    CreatedAt = DateTime.Now
                };

                await _db.SaveUserAsync(user);
                _logger.LogInformation($"Utilisateur créé avec succès: {email}");
                return user;
            }

            _logger.LogInformation($"Utilisateur Azure AD trouvé: {email}");
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur lors de la création/récupération de l'utilisateur Azure: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Vérifie si un utilisateur est membre d'un rôle spécifique
    /// (Peut être étendu pour vérifier les rôles Azure AD)
    /// </summary>
    public async Task<bool> IsUserInRoleAsync(string userId, string role)
    {
        try
        {
            var users = await _db.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
                return false;

            return user.Role == role;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur lors de la vérification du rôle: {ex.Message}");
            return false;
        }
    }
}
