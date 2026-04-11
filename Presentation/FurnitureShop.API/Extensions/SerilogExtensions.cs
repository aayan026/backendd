using Serilog;
using Serilog.Events;

namespace FurnitureShop.API.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Program.cs bootstrap logger — proqram başlamazdan əvvəlki xətalar üçün.
    /// </summary>
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Host Serilog konfiqurasiyası — fayl, console, Seq.
    /// </summary>
    public static void AddSerilogConfiguration(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, services, config) =>
        {
            var seqUrl = context.Configuration["Seq:Url"] ?? "http://localhost:5341";

            config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "FurnitureShop.API")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/furnitureshop-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 50_000_000,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Application}] [Thread:{ThreadId}] {Message:lj}  {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/errors-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}  {Properties:j}{NewLine}{Exception}")
                .WriteTo.Seq(seqUrl);
        });
    }

    /// <summary>
    /// Hər HTTP request-i avtomatik loglar.
    /// </summary>
    public static void UseSerilogHttpLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} => {StatusCode} ({Elapsed:0.000}ms)";

            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode >= 400
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
                if (httpContext.User.Identity?.IsAuthenticated == true)
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
            };
        });
    }
}
