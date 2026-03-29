using FurnitureShop.Application.Dtos.Media;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class LocalFileUploadService : IFileUploadService
{
    private readonly IConfiguration _config;
    private readonly string _uploadRoot;
    private readonly string _baseUrl;

    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; 

    public LocalFileUploadService(IConfiguration config)
    {
        _config = config;
        _uploadRoot = _config["FileUpload:UploadPath"] ?? Path.Combine("wwwroot", "uploads");
        _baseUrl = _config["FileUpload:BaseUrl"] ?? "/uploads";
    }

    public async Task<UploadResultDto> UploadAsync(IFormFile file, string folder = "general")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Fayl boşdur.");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("Faylın maksimum ölçüsü 10 MB-dır.");

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(ext))
            throw new ArgumentException($"Yalnız {string.Join(", ", _allowedExtensions)} formatları qəbul edilir.");

        var folderPath = Path.Combine(_uploadRoot, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(folderPath, uniqueName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return new UploadResultDto
        {
            Url = $"{_baseUrl}/{folder}/{uniqueName}",
            FileName = uniqueName,
            SizeBytes = file.Length
        };
    }

    public Task DeleteAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine("wwwroot", relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch { }
        return Task.CompletedTask;
    }
}
