using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using AzureCarRental.Models;

namespace AzureCarRental.Services;

public interface IImageService
{
    Task<string> UploadAsync(IFormFile file);
    Task DeleteAsync(string url);
}

public class ImageService : IImageService
{
    private readonly BlobContainerClient _container;
    private readonly string _connectionString;
    private string _accountName = "";
    private string _accountKey = "";
    private readonly ILogger<ImageService> _logger;

    public ImageService(IConfiguration config, ILogger<ImageService> logger)
    {
        _logger = logger;
        _connectionString = config.GetConnectionString("AzureStorage") ?? "";

        try
        {
            var client = new BlobServiceClient(_connectionString);
            _container = client.GetBlobContainerClient("images");
            _container.CreateIfNotExists();

            // Extract account name and key from connection string
            ExtractAccountInfo(_connectionString);

            _logger.LogInformation("Azure Storage initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Azure Storage init error: {ex.Message}");
        }
    }

    private void ExtractAccountInfo(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;

                var kvp = part.Split('=', 2);
                if (kvp.Length != 2) continue;

                var key = kvp[0].Trim();
                var value = kvp[1].Trim();

                if (key.Equals("AccountName", StringComparison.OrdinalIgnoreCase))
                    _accountName = value;
                if (key.Equals("AccountKey", StringComparison.OrdinalIgnoreCase))
                    _accountKey = value;
            }
            _logger.LogInformation($"Extracted AccountName: {_accountName}, AccountKey length: {_accountKey?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to extract account info: {ex.Message}");
        }
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return "";

            var name = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blob = _container.GetBlobClient(name);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            // Generate SAS URL for access
            var url = GenerateSasUrl(name);
            _logger.LogInformation($"Image uploaded: {name} -> {url}");
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Upload failed: {ex.Message}");
            return "";
        }
    }

    private string GenerateSasUrl(string blobName)
    {
        try
        {
            if (string.IsNullOrEmpty(_accountName) || string.IsNullOrEmpty(_accountKey))
            {
                _logger.LogWarning($"Account info missing: Name={string.IsNullOrEmpty(_accountName)}, Key={string.IsNullOrEmpty(_accountKey)}. Returning basic URI");
                return _container.GetBlobClient(blobName).Uri.ToString();
            }

            var blobUri = _container.GetBlobClient(blobName).Uri;

            // Create SAS builder
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _container.Name,
                BlobName = blobName,
                Resource = "b", // Blob resource
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(365) // Valid for 1 year
            };

            // Add Read permission
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Create credential
            var credential = new StorageSharedKeyCredential(_accountName, _accountKey);

            // Generate SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            var sasUrl = $"{blobUri}?{sasToken}";
            _logger.LogInformation($"Generated SAS URL for {blobName}: {sasUrl}");
            return sasUrl;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"SAS generation failed: {ex.Message}, returning basic URI");
            return _container.GetBlobClient(blobName).Uri.ToString();
        }
    }

    public async Task DeleteAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try
        {
            // Remove SAS token if present
            var cleanUrl = url.Split('?')[0];
            var uri = new Uri(cleanUrl);
            var name = Path.GetFileName(uri.LocalPath);
            await _container.DeleteBlobAsync(name);
            _logger.LogInformation($"Image deleted: {name}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Delete failed: {ex.Message}");
        }
    }
}
