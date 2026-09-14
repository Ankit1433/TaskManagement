using TaskManagementApi.Models;

namespace TaskManagementApi.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<List<TaskComment>> GetByTaskIdAsync(int taskId);

    Task<int> CreateAsync(TaskComment comment);

    Task<TaskComment?> GetByIdAsync(int id);
}