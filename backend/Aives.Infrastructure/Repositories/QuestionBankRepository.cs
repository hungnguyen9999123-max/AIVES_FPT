using Aives.Application.Interfaces.Repositories;
using Aives.Domain.Entities;
using Aives.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Repositories;

public class QuestionBankRepository : IQuestionBankRepository
{
    private readonly AivesDbContext _db;

    public QuestionBankRepository(AivesDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IEnumerable<QuestionBank>> GetRandomByLevelAsync(
        int courseId, int level, int count)
    {
        if (count <= 0) return [];

        // EF.Functions.Random() → random() tại PostgreSQL, không load toàn bộ bảng
        return await _db.QuestionBanks
            .AsNoTracking()
            .Where(q => q.CourseId == courseId && q.Level == level)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();
    }
}
