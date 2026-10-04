using System.ComponentModel.DataAnnotations;

namespace Aives.Application.Auth.DTOs;

public class RegisterRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}
