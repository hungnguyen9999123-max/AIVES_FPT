using Aives.Domain.Enums;

namespace Aives.Application.DTOs;

/// <summary>Dữ liệu trả về cho MarkdownDocument.</summary>
public record DocumentDto(
    int      DocumentId,
    int      CourseId,
    string   CourseName,
    int      UploadedBy,
    string   FileName,
    string   DocType,
    DateTime? UploadedAt
);

/// <summary>
/// Teacher upload 1 file .md.
/// Validation thực hiện ở service layer (extension, size, nội dung không rỗng).
/// </summary>
public record UploadDocumentRequest(
    int      CourseId,
    int      UploadedBy,
    string   FileName,
    DocType  DocType,
    string   Content       // nội dung text của file .md
);
