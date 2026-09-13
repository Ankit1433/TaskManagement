using TaskManagementApi.DTOs.Tasks;

namespace TaskManagementApi.Services.Interfaces;

public interface ITaskService
{
    Task<List<TaskDetailsResponse>> GetAllAsync();

    Task<TaskDetailsResponse?> GetByIdAsync(int id);

    Task<TaskResponse> CreateAsync(CreateTaskRequest request);

    Task<bool> UpdateAsync(int id, UpdateTaskRequest request);

    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStatusAsync(int id, UpdateTaskStatusRequest request);
}