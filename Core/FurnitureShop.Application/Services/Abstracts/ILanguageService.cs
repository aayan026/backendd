namespace FurnitureShop.Application.Services.Abstracts;

public interface ILanguageService
{
    // Header-dən aktiv dili qaytarır, dəstəklənmirsə default "az"
    string GetCurrentLanguage();

    // Dil kodunun dəstəklenib desteklenmedigini yoxlayir
    bool IsSupported(string lang);

    // Bütün dəstəklənən dilləri qaytarır
    IReadOnlyList<string> GetSupportedLanguages();
}
