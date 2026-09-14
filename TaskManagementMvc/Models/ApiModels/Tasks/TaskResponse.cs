namespace TaskManagementMvc.Models.ApiModels.Tasks;

public class TaskResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedTo { get; set; }

    public int? CreatedBy { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}