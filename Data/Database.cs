using AzureCarRental.Models;
using MySqlConnector;

namespace AzureCarRental.Data;

public class Database
{
    private readonly string _conn;
    private readonly ILogger<Database> _logger;

    private static readonly List<Car> DemoCars = new()
    {
        new Car { Id = 1, Brand = "BMW", Model = "Série 3", Year = 2023, DailyRate = 89.99m, ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400", IsAvailable = true },
        new Car { Id = 2, Brand = "Mercedes", Model = "Classe C", Year = 2022, DailyRate = 95.00m, ImageUrl = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400", IsAvailable = true },
        new Car { Id = 3, Brand = "Audi", Model = "A4", Year = 2023, DailyRate = 85.00m, ImageUrl = "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400", IsAvailable = true },
        new Car { Id = 4, Brand = "Tesla", Model = "Model 3", Year = 2024, DailyRate = 120.00m, ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=400", IsAvailable = false },
        new Car { Id = 5, Brand = "Porsche", Model = "911", Year = 2023, DailyRate = 250.00m, ImageUrl = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400", IsAvailable = true },
        new Car { Id = 6, Brand = "Renault", Model = "Clio", Year = 2022, DailyRate = 45.00m, ImageUrl = "https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=400", IsAvailable = true }
    };

    public Database(IConfiguration config, ILogger<Database> logger)
    {
        _logger = logger;
        _conn = config.GetConnectionString("Default") ?? "";
    }

    public async Task InitAsync()
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();

            var sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    Password VARCHAR(100) NOT NULL,
                    Role VARCHAR(20) NOT NULL DEFAULT 'User',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS Cars (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Brand VARCHAR(50) NOT NULL,
                    Model VARCHAR(50) NOT NULL,
                    Year INT NOT NULL,
                    DailyRate DECIMAL(10,2) NOT NULL,
                    ImageUrl VARCHAR(500),
                    IsAvailable BOOLEAN DEFAULT TRUE
                );
                CREATE TABLE IF NOT EXISTS Reservations (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    CarId INT NOT NULL,
                    CustomerName VARCHAR(100) NOT NULL,
                    CustomerEmail VARCHAR(100) NOT NULL,
                    StartDate DATE NOT NULL,
                    EndDate DATE NOT NULL,
                    TotalPrice DECIMAL(10,2) NOT NULL,
                    FOREIGN KEY (CarId) REFERENCES Cars(Id)
                );";

            using var cmd = new MySqlCommand(sql, c);
            await cmd.ExecuteNonQueryAsync();

            // Admin par défaut
            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Email = 'admin@carrental.com'", c);
            if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) == 0)
            {
                using var insertCmd = new MySqlCommand("INSERT INTO Users (Name, Email, Password, Role) VALUES ('Admin', 'admin@carrental.com', 'Admin123!', 'Admin')", c);
                await insertCmd.ExecuteNonQueryAsync();
            }

            // Voitures par défaut
            using var checkCarsCmd = new MySqlCommand("SELECT COUNT(*) FROM Cars", c);
            if (Convert.ToInt32(await checkCarsCmd.ExecuteScalarAsync()) == 0)
            {
                using var insertCarsCmd = new MySqlCommand(@"
                    INSERT INTO Cars (Brand, Model, Year, DailyRate, ImageUrl, IsAvailable) VALUES 
                    ('BMW', 'Série 3', 2023, 89.99, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400', 1),
                    ('Mercedes', 'Classe C', 2022, 95.00, 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400', 1),
                    ('Audi', 'A4', 2023, 85.00, 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400', 1),
                    ('Tesla', 'Model 3', 2024, 120.00, 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=400', 0),
                    ('Porsche', '911', 2023, 250.00, 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400', 1),
                    ('Renault', 'Clio', 2022, 45.00, 'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=400', 1)
                ", c);
                await insertCarsCmd.ExecuteNonQueryAsync();
            }

            _logger.LogInformation("✓ Database initialized");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"⚠️ DB warning: {ex.Message}");
        }
    }

    public async Task<List<Car>> GetCarsAsync()
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            var cars = new List<Car>();
            using var cmd = new MySqlCommand("SELECT Id,Brand,Model,Year,DailyRate,ImageUrl,IsAvailable FROM Cars", c);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cars.Add(new Car
                {
                    Id = reader.GetInt32(0),
                    Brand = reader.GetString(1),
                    Model = reader.GetString(2),
                    Year = reader.GetInt32(3),
                    DailyRate = reader.GetDecimal(4),
                    ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsAvailable = reader.GetBoolean(6)
                });
            }
            return cars;
        }
        catch
        {
            return DemoCars;
        }
    }

    public async Task<Car?> GetCarAsync(int id)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT Id,Brand,Model,Year,DailyRate,ImageUrl,IsAvailable FROM Cars WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Car
                {
                    Id = reader.GetInt32(0),
                    Brand = reader.GetString(1),
                    Model = reader.GetString(2),
                    Year = reader.GetInt32(3),
                    DailyRate = reader.GetDecimal(4),
                    ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsAvailable = reader.GetBoolean(6)
                };
            }
        }
        catch
        {
            return DemoCars.FirstOrDefault(c => c.Id == id);
        }
        return null;
    }

    public async Task<int> SaveCarAsync(Car car)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            var sql = car.Id == 0
                ? "INSERT INTO Cars(Brand,Model,Year,DailyRate,ImageUrl,IsAvailable) VALUES(@b,@m,@y,@r,@i,@a); SELECT LAST_INSERT_ID();"
                : "UPDATE Cars SET Brand=@b,Model=@m,Year=@y,DailyRate=@r,ImageUrl=@i,IsAvailable=@a WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@b", car.Brand);
            cmd.Parameters.AddWithValue("@m", car.Model);
            cmd.Parameters.AddWithValue("@y", car.Year);
            cmd.Parameters.AddWithValue("@r", car.DailyRate);
            cmd.Parameters.AddWithValue("@i", car.ImageUrl ?? "");
            cmd.Parameters.AddWithValue("@a", car.IsAvailable);
            if (car.Id > 0) cmd.Parameters.AddWithValue("@id", car.Id);
            var result = await cmd.ExecuteScalarAsync();
            return car.Id > 0 ? car.Id : Convert.ToInt32(result);
        }
        catch { }
        return 0;
    }

    public async Task<int> SaveReservationAsync(Reservation res)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("INSERT INTO Reservations(CarId,CustomerName,CustomerEmail,StartDate,EndDate,TotalPrice) VALUES(@car,@name,@email,@start,@end,@price); SELECT LAST_INSERT_ID();", c);
            cmd.Parameters.AddWithValue("@car", res.CarId);
            cmd.Parameters.AddWithValue("@name", res.CustomerName);
            cmd.Parameters.AddWithValue("@email", res.CustomerEmail);
            cmd.Parameters.AddWithValue("@start", res.StartDate);
            cmd.Parameters.AddWithValue("@end", res.EndDate);
            cmd.Parameters.AddWithValue("@price", res.TotalPrice);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch { }
        return 0;
    }

    public async Task<List<Reservation>> GetUserReservationsAsync(string email)
    {
        var reservations = new List<Reservation>();
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT Id, CarId, CustomerName, CustomerEmail, StartDate, EndDate, TotalPrice FROM Reservations WHERE CustomerEmail=@email ORDER BY StartDate DESC", c);
            cmd.Parameters.AddWithValue("@email", email);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reservations.Add(new Reservation
                {
                    Id = reader.GetInt32(0),
                    CarId = reader.GetInt32(1),
                    CustomerName = reader.GetString(2),
                    CustomerEmail = reader.GetString(3),
                    StartDate = reader.GetDateTime(4),
                    EndDate = reader.GetDateTime(5),
                    TotalPrice = reader.GetDecimal(6)
                });
            }
        }
        catch { }
        return reservations;
    }

    public async Task<List<Reservation>> GetAllReservationsAsync()
    {
        var reservations = new List<Reservation>();
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT Id, CarId, CustomerName, CustomerEmail, StartDate, EndDate, TotalPrice FROM Reservations ORDER BY StartDate DESC", c);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reservations.Add(new Reservation
                {
                    Id = reader.GetInt32(0),
                    CarId = reader.GetInt32(1),
                    CustomerName = reader.GetString(2),
                    CustomerEmail = reader.GetString(3),
                    StartDate = reader.GetDateTime(4),
                    EndDate = reader.GetDateTime(5),
                    TotalPrice = reader.GetDecimal(6)
                });
            }
        }
        catch { }
        return reservations;
    }

    public async Task<int> GetTotalCarsAsync()
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Cars", c);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        catch { return DemoCars.Count; }
    }

    public async Task<int> GetAvailableCarsAsync()
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Cars WHERE IsAvailable = TRUE", c);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        catch { return DemoCars.Count(c => c.IsAvailable); }
    }

    public async Task<int> GetTotalReservationsAsync()
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Reservations", c);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        catch { return 0; }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = new List<User>();
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("SELECT Id,Name,Email,Password,Role,CreatedAt FROM Users", c);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Password = reader.GetString(3),
                    Role = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }
        }
        catch { }
        return users;
    }

    public async Task<int> SaveUserAsync(User user)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            var sql = "INSERT INTO Users(Name,Email,Password,Role) VALUES(@name,@email,@password,@role); SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.Parameters.AddWithValue("@role", user.Role);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch { }
        return 0;
    }

    public async Task DeleteCarAsync(int id)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            using var cmd = new MySqlCommand("DELETE FROM Cars WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    public async Task UpdateCarAsync(Car car)
    {
        try
        {
            using var c = new MySqlConnection(_conn);
            await c.OpenAsync();
            var sql = "UPDATE Cars SET Brand=@b,Model=@m,Year=@y,DailyRate=@r,ImageUrl=@i,IsAvailable=@a WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", car.Id);
            cmd.Parameters.AddWithValue("@b", car.Brand);
            cmd.Parameters.AddWithValue("@m", car.Model);
            cmd.Parameters.AddWithValue("@y", car.Year);
            cmd.Parameters.AddWithValue("@r", car.DailyRate);
            cmd.Parameters.AddWithValue("@i", car.ImageUrl ?? "");
            cmd.Parameters.AddWithValue("@a", car.IsAvailable);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }
}
