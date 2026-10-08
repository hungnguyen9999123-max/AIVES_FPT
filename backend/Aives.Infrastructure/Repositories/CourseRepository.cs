using Aives.Application.Exams.Interfaces;
using Aives.Domain.Entities;
using Aives.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AivesDbContext _db;

    public CourseRepository(AivesDbContext db)
    {
        _db = db;
    }

    public async Task<Course?> GetByIdAsync(int courseId)
    {
        return await _db.Courses
            .Include(c => c.Teacher)
            .Include(c => c.CreatedByNavigation)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }
}