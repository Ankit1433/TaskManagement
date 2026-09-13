using Npgsql;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;

namespace TaskManagementApi.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CommentRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<TaskComment>> GetByTaskIdAsync(int taskId)
    {
        var comments = new List<TaskComment>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id,
                   task_id,
                   user_id,
                   comment,
                   created_at
            FROM task_comments
            WHERE task_id = @task_id
            ORDER BY created_at;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("task_id", taskId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            comments.Add(new TaskComment
            {
                Id = reader.GetInt32(0),
                TaskId = reader.GetInt32(1),
                UserId = reader.GetInt32(2),
                Comment = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return comments;
    }

    public async Task<int> CreateAsync(TaskComment comment)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO task_comments
            (
                task_id,
                user_id,
                comment
            )
            VALUES
            (
                @task_id,
                @user_id,
                @comment
            )
            RETURNING id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("task_id", comment.TaskId);
        command.Parameters.AddWithValue("user_id", comment.UserId);
        command.Parameters.AddWithValue("comment", comment.Comment);

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}