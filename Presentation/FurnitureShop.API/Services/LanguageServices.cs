using FurnitureShop.Application.Services.Abstracts;

namespace FurnitureShop.API.Services;

public class LanguageService : ILanguageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly List<string> _supportedLanguages = new() { "az", "ru", "en" };
    private const string _defaultLanguage = "az";

    public LanguageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentLanguage()
    {
        var lang = _httpContextAccessor.HttpContext?
            .Request.Headers["Accept-Language"]
            .FirstOrDefault()
            ?.ToLower()
            ?.Trim();

        return IsSupported(lang!) ? lang! : _defaultLanguage;
    }

    public bool IsSupported(string lang)
        => !string.IsNullOrWhiteSpace(lang) && _supportedLanguages.Contains(lang.ToLower());

    public IReadOnlyList<string> GetSupportedLanguages()
        => _supportedLanguages.AsReadOnly();
}
