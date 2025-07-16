using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using CarSeek.Application.Common.Interfaces;

namespace CarSeek.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _baseUrl;

    public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5193";
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] fileData, string folderPath, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEBUG] Uploading file: {fileName} to folder: {folderPath}");

        // Ensure WebRootPath exists
        if (string.IsNullOrEmpty(_environment.WebRootPath))
        {
            _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(_environment.WebRootPath);
        }

        Console.WriteLine($"[DEBUG] WebRootPath: {_environment.WebRootPath}");

        // Create the directory if it doesn't exist
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderPath);
        Directory.CreateDirectory(uploadsFolder);

        Console.WriteLine($"[DEBUG] Uploads folder: {uploadsFolder}");

        // Save the file
        var filePath = Path.Combine(uploadsFolder, fileName);
        await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

        Console.WriteLine($"[DEBUG] File saved to: {filePath}");

        // Return the URL to access the file
        var imageUrl = $"{_baseUrl}/uploads/{folderPath}/{fileName}";
        Console.WriteLine($"[DEBUG] Generated URL: {imageUrl}");

        return imageUrl;
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract the file path from the URL
            var uri = new Uri(fileUrl);
            var relativePath = uri.AbsolutePath.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
