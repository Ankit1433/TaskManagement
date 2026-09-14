namespace TaskManagementMvc.Models.ApiModels.Comments;

public class CommentResponse
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}