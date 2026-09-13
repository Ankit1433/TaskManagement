using TaskManagementApi.DTOs.Tasks;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;
using TaskManagementApi.Services.Interfaces;

namespace TaskManagementApi.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public TaskService(ITaskRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }



    public async Task<List<TaskDetailsResponse>> GetAllAsync()
    {
        List<TaskItem> tasks;

        if (_currentUser.Role == "Employee")
        {
            tasks = await _repository
                .GetTasksForEmployeeAsync(_currentUser.UserId);
        }
        else
        {
            tasks = await _repository.GetTasksForManagerAsync();
        }

        var result = new List<TaskDetailsResponse>();

        foreach (var task in tasks)
        {
            var isOwnTask = task.CreatedBy == _currentUser.UserId;

            var isAssignedToCurrentEmployee =
                task.AssignedTo == _currentUser.UserId;

            if (isOwnTask || isAssignedToCurrentEmployee)
            {
                result.Add(MapToDetailsResponse(task));
            }
            else
            {
                result.Add(new TaskDetailsResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Status = task.Status
                });
            }
        }

        return result;
    }

    public async Task<TaskDetailsResponse?> GetByIdAsync(int id)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return null;
        }

        var isCreator = task.CreatedBy == _currentUser.UserId;
        var isAssignedEmployee = task.AssignedTo == _currentUser.UserId; ;

        if (isCreator || isAssignedEmployee)
        {
            return new TaskDetailsResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssignedTo = task.AssignedTo,
                CreatedBy = task.CreatedBy,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };
        }

        return new TaskDetailsResponse
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status
        };
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            AssignedTo = request.AssignedTo,
            CreatedBy = _currentUser.UserId,
            Status = "Pending",
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        var id = await _repository.CreateAsync(task);

        task.Id = id;

        var createdTask = await _repository.GetByIdAsync(id);

        return MapToResponse(createdTask!);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateTaskRequest request)
    {
        var existingTask = await _repository.GetByIdAsync(id);

        if (existingTask == null)
        {
            return false;
        }

        existingTask.Title = request.Title;
        existingTask.Description = request.Description;
        existingTask.AssignedTo = request.AssignedTo;
        existingTask.Priority = request.Priority;
        existingTask.DueDate = request.DueDate;

        return await _repository.UpdateAsync(existingTask);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            AssignedTo = task.AssignedTo,
            CreatedBy = task.CreatedBy,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt
        };
    }

    private static TaskDetailsResponse MapToDetailsResponse(TaskItem task)
    {
        return new TaskDetailsResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            AssignedTo = task.AssignedTo,
            CreatedBy = task.CreatedBy,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt
        };
    }

    public async Task<bool> UpdateStatusAsync(
    int id,
    UpdateTaskStatusRequest request)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        var canUpdate =
            task.CreatedBy == _currentUser.UserId ||
            task.AssignedTo == _currentUser.UserId;

        if (!canUpdate)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to update this task.");
        }

        var newStatus = request.Status;

        if (task.Status == "Pending" &&
            newStatus != "In Progress")
        {
            throw new ArgumentException(
                "A pending task can only move to In Progress.");
        }

        if (task.Status == "In Progress" &&
            newStatus != "Completed")
        {
            throw new ArgumentException(
                "An in-progress task can only move to Completed.");
        }

        if (task.Status == "Completed")
        {
            throw new ArgumentException(
                "A completed task cannot be changed.");
        }

        return await _repository.UpdateStatusAsync(
            id,
            request.Status, _currentUser.UserId);
    }
}
