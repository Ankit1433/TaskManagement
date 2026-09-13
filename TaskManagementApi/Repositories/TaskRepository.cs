using Npgsql;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;

namespace TaskManagementApi.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public TaskRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<TaskItem>> GetAllAsync(int userId)
    {
        var tasks = new List<TaskItem>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id,
                   title,
                   description,
                   assigned_to,
                   created_by,
                   status,
                   priority,
                   due_date,
                   created_at
            FROM tasks
            WHERE created_by = @user_id
            OR assigned_to = @user_id
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }

        return tasks;
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT id,
                   title,
                   description,
                   assigned_to,
                   created_by,
                   status,
                   priority,
                   due_date,
                   created_at
            FROM tasks
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapTask(reader);
    }

    public async Task<int> CreateAsync(TaskItem task)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO tasks
            (
                title,
                description,
                assigned_to,
                created_by,
                status,
                priority,
                due_date
            )
            VALUES
            (
                @title,
                @description,
                @assigned_to,
                @created_by,
                @status,
                @priority,
                @due_date
            )
            RETURNING id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("title", task.Title);
        command.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("assigned_to", task.AssignedTo);
        command.Parameters.AddWithValue("created_by", task.CreatedBy);
        command.Parameters.AddWithValue("status", task.Status);
        command.Parameters.AddWithValue("priority", task.Priority);
        command.Parameters.AddWithValue("due_date", (object?)task.DueDate ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(TaskItem task)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            UPDATE tasks
            SET title = @title,
                description = @description,
                assigned_to = @assigned_to,
                priority = @priority,
                due_date = @due_date
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", task.Id);
        command.Parameters.AddWithValue("title", task.Title);
        command.Parameters.AddWithValue(
            "description",
            (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("assigned_to", task.AssignedTo);
        command.Parameters.AddWithValue("priority", task.Priority);
        command.Parameters.AddWithValue(
            "due_date",
            (object?)task.DueDate ?? DBNull.Value);

        var affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
            DELETE FROM tasks
            WHERE id = @id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        var affectedRows = await command.ExecuteNonQueryAsync();

        return affectedRows > 0;
    }

    private static TaskItem MapTask(NpgsqlDataReader reader)
    {
        return new TaskItem
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2)
                ? null
                : reader.GetString(2),
            AssignedTo = reader.GetInt32(3),
            CreatedBy = reader.GetInt32(4),
            Status = reader.GetString(5),
            Priority = reader.GetString(6),
            DueDate = reader.IsDBNull(7)
                ? null
                : reader.GetDateTime(7),
            CreatedAt = reader.GetDateTime(8)
        };
    }

    public async Task<List<TaskItem>> GetTasksForEmployeeAsync(int employeeId)
    {
        var tasks = new List<TaskItem>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
        SELECT id,
               title,
               description,
               assigned_to,
               created_by,
               status,
               priority,
               due_date,
               created_at
        FROM tasks
        WHERE assigned_to = @employee_id
        ORDER BY id;
        """;
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("employee_id", employeeId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }

        return tasks;
    }

    public async Task<List<TaskItem>> GetTasksForManagerAsync()
    {
        var tasks = new List<TaskItem>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
        SELECT id,
               title,
               description,
               assigned_to,
               created_by,
               status,
               priority,
               due_date,
               created_at
        FROM tasks
        ORDER BY id;
        """;
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }

        return tasks;
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        string newStatus,
        int changedBy)
    {
        await using var connection = _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string getStatusSql = """
            SELECT status
            FROM tasks
            WHERE id = @id;
            """;

            await using var getStatusCommand =
                new NpgsqlCommand(
                    getStatusSql,
                    connection,
                    transaction);

            getStatusCommand.Parameters.AddWithValue("id", id);

            var oldStatus =
                await getStatusCommand.ExecuteScalarAsync();

            if (oldStatus == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            const string updateTaskSql = """
            UPDATE tasks
            SET status = @new_status
            WHERE id = @id;
            """;

            await using var updateTaskCommand =
                new NpgsqlCommand(
                    updateTaskSql,
                    connection,
                    transaction);

            updateTaskCommand.Parameters.AddWithValue(
                "id",
                id);

            updateTaskCommand.Parameters.AddWithValue(
                "new_status",
                newStatus);

            await updateTaskCommand.ExecuteNonQueryAsync();

            const string insertHistorySql = """
            INSERT INTO task_status_history
            (
                task_id,
                old_status,
                new_status,
                changed_by
            )
            VALUES
            (
                @task_id,
                @old_status,
                @new_status,
                @changed_by
            );
            """;

            await using var historyCommand =
                new NpgsqlCommand(
                    insertHistorySql,
                    connection,
                    transaction);

            historyCommand.Parameters.AddWithValue(
                "task_id",
                id);

            historyCommand.Parameters.AddWithValue(
                "old_status",
                oldStatus);

            historyCommand.Parameters.AddWithValue(
                "new_status",
                newStatus);

            historyCommand.Parameters.AddWithValue(
                "changed_by",
                changedBy);

            await historyCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}