using TaskManagementApi.Models;

namespace TaskManagementApi.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<int> CreateAsync(User user);

    Task<bool> UpdateAsync(User user);

    Task<bool> DeleteAsync(int id);

    Task<User?> GetByEmailAsync(string email);
}