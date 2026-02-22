using AzureCarRental.Models;
using AzureCarRental.Data;

namespace AzureCarRental.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string email, string password);
    Task<User?> RegisterAsync(string name, string email, string password);
}

public class AuthService : IAuthService
{
    private readonly Database _db;

    public AuthService(Database db)
    {
        _db = db;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        // Búsqueda simple - en producción usar hash
        var users = await _db.GetAllUsersAsync();
        return users.FirstOrDefault(u => u.Email == email && u.Password == password);
    }

    public async Task<User?> RegisterAsync(string name, string email, string password)
    {
        var existingUsers = await _db.GetAllUsersAsync();
        if (existingUsers.Any(u => u.Email == email))
            return null;

        var user = new User { Name = name, Email = email, Password = password, Role = "User", CreatedAt = DateTime.Now };
        // Añadir método para guardar usuario
        return user;
    }
}
