using Npgsql;

namespace Infrastructure41.Configuration;

public class DatabaseOptions
{
    public static string SectionName { get; set; } = "Database";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password,
        };

        return builder.ConnectionString;
    }
}