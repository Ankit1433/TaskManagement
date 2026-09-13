using TaskManagementApi.DTOs.Users;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories.Interfaces;
using TaskManagementApi.Services.Interfaces;
namespace TaskManagementApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly PasswordService _passwordService;

    public UserService(IUserRepository repository, PasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest createUserRequest)
    {
        var user = new User
        {
            Name = createUserRequest.Name,
            Email = createUserRequest.Email,
            Role = createUserRequest.Role,
            PasswordHash = _passwordService.HashPassword(createUserRequest.Password)
        };

        var id = await _repository.CreateAsync(user);

        user.Id = id;

        return MapToResponse(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest updateUserRequest)
    {
        var existingUser = await _repository.GetByIdAsync(id);

        if (existingUser == null)
        {
            return false;
        }
        existingUser.Name = updateUserRequest.Name;
        existingUser.Email = updateUserRequest.Email;
        existingUser.Role = updateUserRequest.Role;

        return await _repository.UpdateAsync(existingUser);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}

