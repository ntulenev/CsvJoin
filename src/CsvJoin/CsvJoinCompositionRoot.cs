using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;
using CsvJoin.Presentation.Console;
using CsvJoin.Presentation.Files;

namespace CsvJoin;

/// <summary>
/// Registers configuration and services for the CSV join application.
/// </summary>
internal static class CsvJoinCompositionRoot
{
    /// <summary>
    /// Adds configuration sources required by the application.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder to configure.</param>
    public static void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    }

    /// <summary>
    /// Registers the application's runtime services.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddCsvJoinServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AppSettings>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidator>();
        services.AddTransient<ICsvJoinQueryParser, CsvJoinQueryParser>();
        services.AddTransient<IConfiguredJoinJobFactory, ConfiguredJoinJobFactory>();
        services.AddTransient<ICsvFileReader, CsvFileReader>();
        services.AddTransient<ICsvJoinProcessor, CsvJoinProcessor>();
        services.AddTransient<IConsoleOutputRenderer, SpectreConsoleOutputRenderer>();
        services.AddTransient<IResultFileWriter, CsvResultFileWriter>();
        services.AddTransient<IResultFileLauncher, ShellResultFileLauncher>();
        services.AddTransient<ICsvJoinApplication, CsvJoinApplication>();

        return services;
    }
}
