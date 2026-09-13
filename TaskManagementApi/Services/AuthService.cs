using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagementApi.DTOs.Auth;
using TaskManagementApi.Repositories.Interfaces;
using TaskManagementApi.Services;
using TaskManagementApi.Services.Interfaces;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        PasswordService passwordService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _configuration = configuration;
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return null;
        }

        var validPassword = _passwordService.VerifyPassword(user.PasswordHash, request.Password);

        if (!validPassword)
        {
            return null;
        }

        var token = GenerateToken(user);
        return new LoginResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Role = user.Role,
            Token = token
        };
    }

    private string GenerateToken(TaskManagementApi.Models.User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}