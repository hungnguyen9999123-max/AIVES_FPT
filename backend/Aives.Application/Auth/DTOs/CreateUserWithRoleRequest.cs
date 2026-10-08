using System.ComponentModel.DataAnnotations;
using Aives.Domain.Enums;

namespace Aives.Application.Auth.DTOs;

/// <summary>
/// Chỉ ADMIN mới được dùng — tạo user với role tuỳ chọn (ADMIN, TEACHER, STUDENT).
/// </summary>
public class CreateUserWithRoleRequest
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

    [Required]
    public UserRole Role { get; set; }
}
