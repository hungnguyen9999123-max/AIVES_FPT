using Aives.Domain.Entities;

namespace Aives.Application.Exams.Interfaces;

public interface IMarkdownDocumentRepository
{
    Task<MarkdownDocument> CreateAsync(MarkdownDocument document);
    Task<IEnumerable<MarkdownDocument>> GetByCourseIdAsync(int courseId);
    Task SaveChangesAsync();
}