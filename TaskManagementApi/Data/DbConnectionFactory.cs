using Npgsql;
namespace TaskManagementApi.Data;
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString=configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
                "Database connection string is missing.");
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}