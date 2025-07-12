using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace NovaPdf.Reporting.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaReport(this IServiceCollection services)
    {
        IPlaywright playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
        services.AddSingleton(playwright);

        return services;
    }
    public static IServiceCollection AddDockerNovaReportSupport(this IServiceCollection services)
    {
        try
        {
            var exitCode = Microsoft.Playwright.Program.Main([ "install", "--with-deps", "chromium" ]);
            if (exitCode != 0)
            {
                throw new Exception($"Playwright installation failed with exit code {exitCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error during Playwright installation: {ex.Message}");
        }

        return services;
    }
}