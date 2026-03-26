using FurnitureShop.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.API.Controllers;

[Route("api/media")]
[Authorize(Roles = "Admin")]
public class MediaController : BaseApiController
{
    private readonly IFileUploadService _uploadService;

    public MediaController(IFileUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    /// <summary>
    /// Şəkil yükləyir — products, collections, categories üçün
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Fayl seçilməyib." });

        var result = await _uploadService.UploadAsync(file, folder);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Şəkili silir
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest(new { error = "URL boş ola bilməz." });

        await _uploadService.DeleteAsync(url);
        return DeletedResponse();
    }
}
