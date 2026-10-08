using Aives.Application.DTOs;

namespace Aives.Application.Interfaces.Services;

public interface IMarkdownDocumentService
{
    Task<IEnumerable<DocumentDto>> GetByCourseIdAsync(int courseId);
    Task<DocumentDto> GetByIdAsync(int documentId);

    /// <summary>
    /// Validate và lưu file .md:
    /// - Extension phải là .md
    /// - Nội dung không được rỗng
    /// - Mỗi Course chỉ được có 1 file mỗi DocType
    /// - Teacher phải được phân công phụ trách Course
    /// </summary>
    Task<DocumentDto> UploadAsync(UploadDocumentRequest request);

    Task DeleteAsync(int documentId, int requestedByUserId);
}
