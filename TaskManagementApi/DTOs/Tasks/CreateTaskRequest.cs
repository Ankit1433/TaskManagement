namespace TaskManagementApi.DTOs.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int AssignedTo { get; set; }

    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }
}