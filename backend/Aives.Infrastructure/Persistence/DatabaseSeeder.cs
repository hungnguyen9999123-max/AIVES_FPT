using Aives.Domain.Entities;
using Aives.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds a teacher account and two sample courses for testing the markdown upload feature.
    /// Idempotent — safe to call multiple times; skips data that already exists.
    /// </summary>
    public static async Task SeedAsync(AivesDbContext db, string hashPassword)
    {
        // ------------------------------------------------------------------ //
        //  1. Teacher account                                                  //
        // ------------------------------------------------------------------ //
        const string teacherUsername = "teacher_test";

        var teacher = await db.Users
            .FirstOrDefaultAsync(u => u.Username == teacherUsername);

        if (teacher is null)
        {
            teacher = new User
            {
                Username   = teacherUsername,
                FullName   = "Nguyễn Văn Giảng",
                Email      = "teacher_test@aives.edu.vn",
                PasswordHash = hashPassword,
                Role       = UserRole.TEACHER,
                CreatedAt  = DateTime.UtcNow
            };
            db.Users.Add(teacher);
            await db.SaveChangesAsync(); // flush to get UserId
        }

        // ------------------------------------------------------------------ //
        //  2. Courses (assign teacher as both TeacherId and CreatedBy)         //
        // ------------------------------------------------------------------ //
        var coursesToSeed = new[]
        {
            new { Code = "CS101", Name = "Nhập môn Lập trình" },
            new { Code = "SE201", Name = "Kỹ nghệ Phần mềm" }
        };

        foreach (var c in coursesToSeed)
        {
            var exists = await db.Courses.AnyAsync(x => x.CourseCode == c.Code);
            if (!exists)
            {
                db.Courses.Add(new Course
                {
                    CourseCode = c.Code,
                    CourseName = c.Name,
                    TeacherId  = teacher.UserId,
                    CreatedBy  = teacher.UserId,
                    CreatedAt  = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
