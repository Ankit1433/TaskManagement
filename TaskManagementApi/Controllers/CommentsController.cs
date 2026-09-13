using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.DTOs.Comments;
using TaskManagementApi.Services.Interfaces;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/tasks/{taskId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int taskId)
    {
        var comments = await _service.GetByTaskIdAsync(taskId);

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int taskId,
        CreateCommentRequest request)
    {
        var comment = await _service.CreateAsync(
            taskId,
            request);

        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
    }
}