using Application41.Ports.Repo;
using FluentMigrator.Runner;
using Infrastructure41.Configuration;
using Infrastructure41.Persistence.Migrations;
using Infrastructure41.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure41.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddDatabaseInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            DatabaseOptions options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            string connectionString = options.GetConnectionString();
            return NpgsqlDataSource.Create(connectionString);
        });

        return services;
    }

    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                {
                    DatabaseOptions options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    return options.GetConnectionString();
                })
                .ScanIn(typeof(InitialMigration).Assembly)
                .For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();

        return services;
    }

    public static void RunDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        try
        {
            IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run database migrations.");
            throw;
        }
    }
}
