using Aives.Domain.Entities;

namespace Aives.Application.Exams.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int courseId);
}