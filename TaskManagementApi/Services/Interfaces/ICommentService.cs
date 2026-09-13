using TaskManagementApi.DTOs.Comments;

namespace TaskManagementApi.Services.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponse>> GetByTaskIdAsync(int taskId);

    Task<CommentResponse?> CreateAsync(
        int taskId,
        CreateCommentRequest request);
}