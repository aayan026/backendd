using FurnitureShop.Application.Dtos.Media;
using Microsoft.AspNetCore.Http;

namespace FurnitureShop.Application.Services.Abstracts;

public interface IFileUploadService
{
    Task<UploadResultDto> UploadAsync(IFormFile file, string folder = "general");
    Task DeleteAsync(string fileUrl);
}
