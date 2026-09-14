using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.DTOs.Comments;

public class CreateCommentRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Comment { get; set; } = string.Empty;
}