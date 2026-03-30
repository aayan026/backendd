
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
namespace FurnitureShop.Infrastructure.Services.Concretes;


public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config["CloudinarySettings:CloudName"];
        var apiKey = config["CloudinarySettings:ApiKey"];
        var apiSecret = config["CloudinarySettings:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        return result.SecureUrl.ToString();
    }
}


