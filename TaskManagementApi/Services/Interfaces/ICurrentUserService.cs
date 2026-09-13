using System.Security.Claims;

namespace TaskManagementApi.Services.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }

    string Role { get; }
}