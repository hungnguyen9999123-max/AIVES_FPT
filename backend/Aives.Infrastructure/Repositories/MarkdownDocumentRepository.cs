using Aives.Application.Exams.Interfaces;
using Aives.Domain.Entities;
using Aives.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aives.Infrastructure.Repositories;

public class MarkdownDocumentRepository : IMarkdownDocumentRepository
{
    private readonly AivesDbContext _db;

    public MarkdownDocumentRepository(AivesDbContext db)
    {
        _db = db;
    }

    public async Task<MarkdownDocument> CreateAsync(MarkdownDocument document)
    {
        _db.MarkdownDocuments.Add(document);
        await _db.SaveChangesAsync();
        return document;
    }

    public async Task<IEnumerable<MarkdownDocument>> GetByCourseIdAsync(int courseId)
    {
        return await _db.MarkdownDocuments
            .Where(d => d.CourseId == courseId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}