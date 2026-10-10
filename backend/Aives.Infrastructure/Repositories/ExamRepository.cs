using Aives.Application.Interfaces.Repositories;
using Aives.Domain.Entities;
using Aives.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly AivesDbContext _db;

    public ExamRepository(AivesDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IEnumerable<Exam>> GetByCourseIdAsync(int courseId)
        => await _db.Exams
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Exam?> GetWithSessionsAsync(int examId)
        => await _db.Exams
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.ExamSessions)
                .ThenInclude(s => s.Results)
                    .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(e => e.ExamId == examId);

    /// <inheritdoc/>
    public async Task<ExamSession?> GetSessionWithResultsAsync(int sessionId)
        => await _db.ExamSessions
            .AsNoTracking()
            .Include(s => s.Exam)
                .ThenInclude(e => e.Course)
            .Include(s => s.Results)
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

    /// <inheritdoc/>
    public async Task<Result?> GetResultDetailAsync(int resultId)
        => await _db.Results
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.ReviewedByNavigation)
            .Include(r => r.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(r => r.ResultId == resultId);
}
