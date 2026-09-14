using System.ComponentModel.DataAnnotations;

namespace TaskManagementMvc.Models.ApiModels.Tasks;

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
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }
}