using System.Security.Claims;
using TaskManagementApi.Services.Interfaces;
namespace TaskManagementApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(value!);
        }
    }

    public string Role
    {
        get
        {
            return _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.Role)
                ?? string.Empty;
        }
    }
}