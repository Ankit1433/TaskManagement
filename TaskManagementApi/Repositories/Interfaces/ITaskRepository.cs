using TaskManagementApi.Models;

namespace TaskManagementApi.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(int userId);

    Task<TaskItem?> GetByIdAsync(int id);

    Task<int> CreateAsync(TaskItem task);

    Task<bool> UpdateAsync(TaskItem task);

    Task<bool> DeleteAsync(int id);

    Task<List<TaskItem>> GetTasksForEmployeeAsync(int employeeId);

    Task<List<TaskItem>> GetTasksForManagerAsync();
    Task<bool> UpdateStatusAsync(int id, string status,int changedBy);
}