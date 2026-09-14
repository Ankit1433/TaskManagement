using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.DTOs.Users;

public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Manager|Employee)$",
        ErrorMessage = "Role must be Manager or Employee.")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}