using TaskManagementApi.DTOs.Auth;
namespace TaskManagementApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}