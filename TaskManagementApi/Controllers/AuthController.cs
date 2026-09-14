using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Services.Interfaces;
using TaskManagementApi.Services;
using TaskManagementApi.DTOs.Auth;
namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(result);
    }
}