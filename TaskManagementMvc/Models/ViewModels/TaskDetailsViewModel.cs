using TaskManagementMvc.Models.ApiModels.Comments;
using TaskManagementMvc.Models.ApiModels.Tasks;

namespace TaskManagementMvc.Models.ViewModels;

public class TaskDetailsViewModel
{
    public TaskDetailsResponse Task { get; set; } = new();

    public List<CommentResponse> Comments { get; set; } = new();
}