namespace FurnitureShop.Application.Services.Abstracts;

public interface ILanguageService
{
    string GetCurrentLanguage();

    bool IsSupported(string lang);

    IReadOnlyList<string> GetSupportedLanguages();
}
