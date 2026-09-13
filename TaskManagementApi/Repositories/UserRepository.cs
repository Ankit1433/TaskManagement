using Npgsql;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
namespace TaskManagementApi.Repositories.Interfaces;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task<int> CreateAsync(User user)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        const string sql = """
            INSERT INTO users (name, email, role,password_hash)
            VALUES (@name, @email, @role,@password_hash)
            RETURNING id;
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("role", user.Role);
        command.Parameters.AddWithValue("password_hash", user.PasswordHash);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            DELETE FROM users
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        var affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }
    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
        select id,name,email,role
        from users
        order by id;
        """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }
    private static User MapUser(NpgsqlDataReader reader) => new User
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2),
        Role = reader.GetString(3)
    };
    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string query = """
         SELECT id, name, email, role
            FROM users
            WHERE id = @id; 
        """;

        await using var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }
        return MapUser(reader);
    }
    public async Task<bool> UpdateAsync(User user)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            UPDATE users
            SET name = @name,
                email = @email,
                role = @role
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("role", user.Role);

        var affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string query = """
        SELECT id, name, email, role, password_hash
        FROM users
        WHERE email = @email;
        """;

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("email", email);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Role = reader.GetString(3),
            PasswordHash = reader.GetString(4)

        };
    }
}