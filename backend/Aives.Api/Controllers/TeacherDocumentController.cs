using Aives.Application.DTOs;
using Aives.Application.Interfaces.Services;
using Aives.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Aives.Api.Controllers;

/// <summary>
/// Teacher upload và quản lý file .md liên kết với Course.
/// Base route: /api/documents
/// </summary>
[ApiController]
[Route("api/documents")]
public class TeacherDocumentController : ControllerBase
{
    private readonly IMarkdownDocumentService _docService;

    public TeacherDocumentController(IMarkdownDocumentService docService) => _docService = docService;

    // GET /api/documents/course/{courseId}
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetByCourse(int courseId)
    {
        var docs = await _docService.GetByCourseIdAsync(courseId);
        return Ok(docs);
    }

    // GET /api/documents/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var doc = await _docService.GetByIdAsync(id);
            return Ok(doc);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload file .md từ multipart/form-data.
    /// Form fields: courseId, uploadedBy, docType (CONTENT | LEARNING_OUTCOMES).
    /// File field: file (phải có extension .md).
    /// </summary>
    // POST /api/documents/upload
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] int    courseId,
        [FromForm] int    uploadedBy,
        [FromForm] string docType,
        IFormFile         file)
    {
        // Validate file có được gửi lên không
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });

        // Parse docType enum
        if (!Enum.TryParse<DocType>(docType.ToUpper(), out var parsedDocType))
            return BadRequest(new { message = $"Invalid docType '{docType}'. Valid values: CONTENT, LEARNING_OUTCOMES." });

        // Đọc nội dung file thành string (service sẽ validate extension, size, content)
        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            content = await reader.ReadToEndAsync();
        }

        var request = new UploadDocumentRequest(
            CourseId:   courseId,
            UploadedBy: uploadedBy,
            FileName:   file.FileName,
            DocType:    parsedDocType,
            Content:    content
        );

        try
        {
            var created = await _docService.UploadAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.DocumentId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Xóa document. Query param: requestedBy (userId của người thực hiện).
    /// Chỉ Teacher upload hoặc Admin mới được xóa.
    /// </summary>
    // DELETE /api/documents/{id}?requestedBy={userId}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int requestedBy)
    {
        try
        {
            await _docService.DeleteAsync(id, requestedBy);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
