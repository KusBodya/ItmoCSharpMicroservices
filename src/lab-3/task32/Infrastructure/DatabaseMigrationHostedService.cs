using Infrastructure.DependencyInjection;

namespace Task32.Infrastructure;

public class DatabaseMigrationHostedService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationHostedService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            serviceProvider.RunDatabaseMigrations();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database migrations failed during startup.");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}