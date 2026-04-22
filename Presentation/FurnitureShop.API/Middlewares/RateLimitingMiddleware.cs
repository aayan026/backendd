using System.Collections.Concurrent;
using System.Text.Json;

namespace FurnitureShop.API.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    private static readonly ConcurrentDictionary<string, (DateTime WindowStart, int Count)> _store = new();

    private static readonly Dictionary<string, (int Max, TimeSpan Window)> _rules = new()
    {
        { "/api/auth/login",           (15, TimeSpan.FromMinutes(15)) },
        { "/api/auth/register",        (10,  TimeSpan.FromMinutes(15)) },
        { "/api/auth/forgot-password", (5,  TimeSpan.FromHours(1))    },
        { "/api/auth/refresh",         (50, TimeSpan.FromMinutes(15)) },
        { "/api/auth/google",          (10, TimeSpan.FromMinutes(15)) },
    };

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        if (!_rules.TryGetValue(path, out var rule))
        {
            await _next(context);
            return;
        }

        var ip  = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{ip}:{path}";
        var now = DateTime.UtcNow;

        _store.AddOrUpdate(
            key,
            _ => (now, 1),
            (_, existing) =>
            {
                if (now - existing.WindowStart > rule.Window)
                    return (now, 1);
                return (existing.WindowStart, existing.Count + 1);
            });

        var current = _store[key];

        if (current.Count > rule.Max)
        {
            _logger.LogWarning("Rate limit — IP: {IP} Path: {Path} Count: {Count}/{Max}",
                ip, path, current.Count, rule.Max);

            var retryAfter = (int)(rule.Window - (now - current.WindowStart)).TotalSeconds;
            context.Response.StatusCode  = 429;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Retry-After"] = retryAfter.ToString();

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success    = false,
                statusCode = 429,
                message    = "Çox sayda sorğu göndərildi. Bir az sonra yenidən cəhd edin.",
                retryAfter
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

            return;
        }

        await _next(context);
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthRateLimiting(this IApplicationBuilder app)
        => app.UseMiddleware<RateLimitingMiddleware>();
}
