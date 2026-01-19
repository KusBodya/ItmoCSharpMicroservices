using Npgsql;

#pragma warning disable CA2100
namespace Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase
{
    protected RepositoryBase(NpgsqlDataSource dataSource)
    {
        DataSource = dataSource;
    }

    protected NpgsqlDataSource DataSource { get; }

    protected async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return await DataSource.OpenConnectionAsync(cancellationToken);
    }
}
