using TaskManagementApi.DTOs.Tasks;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;
using TaskManagementApi.Services.Interfaces;

namespace TaskManagementApi.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;

    public TaskService(
        ITaskRepository repository,
        ICurrentUserService currentUser,
        IUserRepository userRepository)
    {
        _repository = repository;
        _currentUser = currentUser;
        _userRepository = userRepository;
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
            tasks = await _repository
                .GetTasksForManagerAsync();
        }

        var result = new List<TaskDetailsResponse>();

        foreach (var task in tasks)
        {
            var isOwnTask =
                task.CreatedBy == _currentUser.UserId;

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

        var isCreator =
            task.CreatedBy == _currentUser.UserId;

        var isAssignedEmployee =
            task.AssignedTo == _currentUser.UserId;

        if (isCreator || isAssignedEmployee)
        {
            return MapToDetailsResponse(task);
        }

        return new TaskDetailsResponse
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status
        };
    }

    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request)
    {
        // Only managers can create tasks.
        if (_currentUser.Role != "Manager")
        {
            throw new UnauthorizedAccessException(
                "Only managers can create tasks.");
        }

        // Verify assigned user exists.
        var assignedUser =
            await _userRepository.GetByIdAsync(
                request.AssignedTo);

        if (assignedUser == null)
        {
            throw new KeyNotFoundException(
                "Assigned user was not found.");
        }

        // A task can only be assigned to an employee.
        if (assignedUser.Role != "Employee")
        {
            throw new ArgumentException(
                "Tasks can only be assigned to employees.");
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            AssignedTo = request.AssignedTo,

            // Comes from JWT, not from the request.
            CreatedBy = _currentUser.UserId,

            Status = "Pending",
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        var id = await _repository.CreateAsync(task);

        var createdTask =
            await _repository.GetByIdAsync(id);

        if (createdTask == null)
        {
            throw new InvalidOperationException(
                "Task was created but could not be retrieved.");
        }

        return MapToResponse(createdTask);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateTaskRequest request)
    {
        var existingTask =
            await _repository.GetByIdAsync(id);

        if (existingTask == null)
        {
            return false;
        }

        // Only the creator can edit task details.
        if (existingTask.CreatedBy != _currentUser.UserId)
        {
            throw new UnauthorizedAccessException(
                "Only the task creator can edit task details.");
        }

        // Verify the new assignee.
        var assignedUser =
            await _userRepository.GetByIdAsync(
                request.AssignedTo);

        if (assignedUser == null)
        {
            throw new KeyNotFoundException(
                "Assigned user was not found.");
        }

        if (assignedUser.Role != "Employee")
        {
            throw new ArgumentException(
                "Tasks can only be assigned to employees.");
        }

        existingTask.Title = request.Title.Trim();

        existingTask.Description =
            request.Description?.Trim();

        existingTask.AssignedTo =
            request.AssignedTo;

        existingTask.Priority =
            request.Priority;

        existingTask.DueDate =
            request.DueDate;

        return await _repository.UpdateAsync(
            existingTask);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task =
            await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        // Only creator can delete.
        if (task.CreatedBy != _currentUser.UserId)
        {
            throw new UnauthorizedAccessException(
                "Only the task creator can delete the task.");
        }

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        UpdateTaskStatusRequest request)
    {
        var task =
            await _repository.GetByIdAsync(id);

        if (task == null)
        {
            return false;
        }

        // Creator or assigned employee can change status.
        var canUpdate =
            task.CreatedBy == _currentUser.UserId ||
            task.AssignedTo == _currentUser.UserId;

        if (!canUpdate)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to update this task.");
        }

        var newStatus = request.Status.Trim();

        // Business rule belongs in the service.
        if (!IsValidStatusTransition(
                task.Status,
                newStatus))
        {
            throw new ArgumentException(
                $"Invalid status transition from " +
                $"{task.Status} to {newStatus}.");
        }

        return await _repository.UpdateStatusAsync(
            id,
            newStatus,
            _currentUser.UserId);
    }

    private static bool IsValidStatusTransition(
        string currentStatus,
        string newStatus)
    {
        return currentStatus switch
        {
            "Pending" =>
                newStatus == "In Progress",

            "In Progress" =>
                newStatus == "Completed",

            "Completed" =>
                false,

            _ =>
                false
        };
    }

    private static TaskResponse MapToResponse(
        TaskItem task)
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

    private static TaskDetailsResponse MapToDetailsResponse(
        TaskItem task)
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
}