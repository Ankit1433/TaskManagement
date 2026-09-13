using TaskManagementApi.DTOs.Users;
namespace TaskManagementApi.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);

    Task<UserResponse> CreateAsync(CreateUserRequest createUserRequest);

    Task<bool> DeleteAsync(int id);

    Task<bool> UpdateAsync(int id,UpdateUserRequest updateUserRequest);

}