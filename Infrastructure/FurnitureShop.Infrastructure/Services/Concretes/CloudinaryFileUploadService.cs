using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FurnitureShop.Application.Dtos.Media;
using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FurnitureShop.Infrastructure.Services.Concretes;

public class CloudinaryFileUploadService : IFileUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileUploadService> _logger;

    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public CloudinaryFileUploadService(IConfiguration config, ILogger<CloudinaryFileUploadService> logger)
    {
        _logger = logger;

        var cloudName = config["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName konfiqurasiyada tapılmadı.");
        var apiKey = config["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey konfiqurasiyada tapılmadı.");
        var apiSecret = config["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret konfiqurasiyada tapılmadı.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
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

        using var stream = file.OpenReadStream();

        var publicId = $"furnitureshop/{folder}/{Guid.NewGuid()}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Overwrite = false,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary yükləmə xətası: {Error}", result.Error.Message);
            throw new Exception($"Şəkil yüklənərkən xəta baş verdi: {result.Error.Message}");
        }

        _logger.LogInformation("Şəkil Cloudinary-ə yükləndi: {Url}", result.SecureUrl);

        return new UploadResultDto
        {
            Url = result.SecureUrl.ToString(),
            FileName = result.PublicId,
            SizeBytes = file.Length
        };
    }

    public async Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        try
        {
            // Cloudinary URL-dən public_id çıxarırıq
            // Nümunə: https://res.cloudinary.com/{cloud}/image/upload/v123/furnitureshop/products/abc123
            var uri = new Uri(fileUrl);
            var segments = uri.AbsolutePath.Split('/');

            // "upload" sonrasındakı hissə (version + public_id)
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex < 0 || uploadIndex + 2 >= segments.Length)
            {
                _logger.LogWarning("Cloudinary public_id URL-dən çıxarıla bilmədi: {Url}", fileUrl);
                return;
            }

            // Version segmentini atlayırıq (v123456789 formatında olursa)
            var afterUpload = segments.Skip(uploadIndex + 1).ToArray();
            var firstSeg = afterUpload[0];
            var publicIdSegments = firstSeg.StartsWith("v") && firstSeg.Length > 1 && char.IsDigit(firstSeg[1])
                ? afterUpload.Skip(1).ToArray()
                : afterUpload;

            // Son seqmentdən extension-u çıxarırıq
            var lastSeg = publicIdSegments.Last();
            var dotIndex = lastSeg.LastIndexOf('.');
            if (dotIndex > 0)
                publicIdSegments[publicIdSegments.Length - 1] = lastSeg[..dotIndex];

            var publicId = string.Join("/", publicIdSegments);

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result == "ok")
                _logger.LogInformation("Cloudinary-dən şəkil silindi: {PublicId}", publicId);
            else
                _logger.LogWarning("Cloudinary silmə nəticəsi: {Result} — PublicId: {PublicId}", result.Result, publicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary-dən şəkil silinərkən xəta: {Url}", fileUrl);
        }
    }
}
