using FluentMigrator.Runner;
using VmesteGO.Migrations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class FluentMigratorConfiguration
{
    public static IServiceCollection AddFluentMigrator(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        return services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(AddTables).Assembly).For.Migrations())
            .AddLogging(lb =>
            {
                lb.AddFluentMigratorConsole();
            });
    }
}