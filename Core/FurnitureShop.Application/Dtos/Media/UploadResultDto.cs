namespace FurnitureShop.Application.Dtos.Media;

public class UploadResultDto
{
    public string Url { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public long SizeBytes { get; set; }
}
