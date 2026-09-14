using Microsoft.AspNetCore.Mvc;
using TaskManagementMvc.Models.ApiModels.Tasks;
using TaskManagementMvc.Services.Interfaces;
using TaskManagementMvc.Models.ApiModels.Users;
using TaskManagementMvc.Models.ApiModels.Comments;
using TaskManagementMvc.Models.ViewModels;
using TaskManagementMvc.Filters;
namespace TaskManagementMvc.Controllers;

[SessionAuthorize]
public class TaskController : Controller
{
    private readonly IApiClient _apiClient;

    public TaskController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var tasks = await _apiClient.GetAsync<List<TaskResponse>>(
            "api/tasks");

        return View(tasks ?? new List<TaskResponse>());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _apiClient.GetAsync<TaskDetailsResponse>(
            $"api/tasks/{id}");

        if (task == null)
        {
            return NotFound();
        }

        var comments = await _apiClient.GetAsync<List<CommentResponse>>(
            $"api/tasks/{id}/comments");

        var viewModel = new TaskDetailsViewModel
        {
            Task = task,
            Comments = comments ?? new List<CommentResponse>()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var role = HttpContext.Session.GetString("Role");

        if (role != "Manager")
        {
            return Forbid();
        }

        var users = await _apiClient.GetAsync<List<UserResponse>>(
            "api/users");

        var employees = users?
            .Where(x => x.Role == "Employee")
            .ToList()
            ?? new List<UserResponse>();

        ViewBag.Employees = employees;

        return View(new CreateTaskRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    CreateTaskRequest request)
    {
        var role = HttpContext.Session.GetString("Role");

        if (role != "Manager")
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var users = await _apiClient.GetAsync<List<UserResponse>>(
                "api/users");

            ViewBag.Employees = users?
                .Where(x => x.Role == "Employee")
                .ToList()
                ?? new List<UserResponse>();

            return View(request);
        }

        var task = await _apiClient.PostAsync<
            CreateTaskRequest,
            TaskResponse>(
            "api/tasks",
            request);

        if (task == null)
        {
            ModelState.AddModelError(
                "",
                "Task could not be created.");

            return View(request);
        }

        return RedirectToAction(
            "Details",
            new { id = task.Id });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateTaskStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            TempData["Error"] = "Please select a status.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        try
        {
            await _apiClient.PatchAsync(
                $"api/tasks/{id}/status",
                request);

            TempData["Success"] = "Task status updated successfully.";
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"Unable to update status: {ex.Message}";
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(
        int taskId,
        CreateCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            TempData["Error"] = "Comment cannot be empty.";

            return RedirectToAction(
                nameof(Details),
                new { id = taskId });
        }

        try
        {
            await _apiClient.PostAsync<
                CreateCommentRequest,
                CommentResponse>(
                $"api/tasks/{taskId}/comments",
                request);

            TempData["Success"] = "Comment added successfully.";
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] =
                $"Unable to add comment: {ex.Message}";
        }

        return RedirectToAction(
            nameof(Details),
            new { id = taskId });
    }
}