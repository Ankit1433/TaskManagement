using Microsoft.AspNetCore.Identity;
namespace TaskManagementApi.Services;

public class PasswordService
{
    private readonly PasswordHasher<Object> _passwordHasher = new();
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string hashedpassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hashedpassword, password);

        return result == PasswordVerificationResult.Success;
    }
}