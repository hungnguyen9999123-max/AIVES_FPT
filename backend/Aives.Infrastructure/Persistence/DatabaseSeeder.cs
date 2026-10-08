using Aives.Domain.Entities;
using Aives.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private const string AdminUsername = "admin";
    private const string AdminEmail    = "admin@aives.edu.vn";
    private const string AdminPassword = "Admin@123";

    /// <summary>
    /// Tự động seed 1 admin mặc định khi app khởi động.
    /// Idempotent — bỏ qua nếu username đã tồn tại.
    /// Credentials: email=admin@aives.edu.vn / password=Admin@123
    /// </summary>
    public static async Task SeedAdminAsync(AivesDbContext db)
    {
        var exists = await db.Users.AnyAsync(u => u.Username == AdminUsername);
        if (exists) return;

        var admin = new User
        {
            Username     = AdminUsername,
            FullName     = "System Admin",
            Email        = AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword, workFactor: 12),
            Role         = UserRole.ADMIN,
            CreatedAt    = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
