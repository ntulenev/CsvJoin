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

internal static class CsvJoinCompositionRoot
{
    public static void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    }

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
