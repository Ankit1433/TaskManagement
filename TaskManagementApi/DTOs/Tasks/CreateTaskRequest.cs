using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int AssignedTo { get; set; }

    [Required]
    [RegularExpression(
        "^(Low|Medium|High)$",
        ErrorMessage = "Priority must be Low, Medium, or High.")]
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }
}