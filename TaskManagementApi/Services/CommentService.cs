using TaskManagementApi.DTOs.Comments;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;
using TaskManagementApi.Services.Interfaces;

namespace TaskManagementApi.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUser;

    public CommentService(
        ICommentRepository commentRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUser)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _currentUser = currentUser;
    }

    public async Task<List<CommentResponse>> GetByTaskIdAsync(
        int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException("Task not found.");
        }

        EnsureCanAccessTask(task);

        var comments =
            await _commentRepository.GetByTaskIdAsync(taskId);

        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CommentResponse?> CreateAsync(
        int taskId,
        CreateCommentRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task == null)
        {
            return null;
        }

        EnsureCanAccessTask(task);

        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new ArgumentException(
                "Comment cannot be empty.");
        }

        var comment = new TaskComment
        {
            TaskId = taskId,
            UserId = _currentUser.UserId,
            Comment = request.Comment.Trim()
        };

        var id = await _commentRepository.CreateAsync(comment);

        comment.Id = id;
        comment.CreatedAt = DateTime.UtcNow;

        return MapToResponse(comment);
    }

    private void EnsureCanAccessTask(TaskItem task)
    {
        var canAccess =
            task.CreatedBy == _currentUser.UserId ||
            task.AssignedTo == _currentUser.UserId ||
            _currentUser.Role == "Manager";

        if (!canAccess)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to access this task.");
        }
    }

    private static CommentResponse MapToResponse(
        TaskComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            Comment = comment.Comment,
            CreatedAt = comment.CreatedAt
        };
    }
}