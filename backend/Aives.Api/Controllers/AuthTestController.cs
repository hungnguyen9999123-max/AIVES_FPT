using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aives.Api.Controllers;

/// <summary>
/// Simple role-based authorization test endpoints.
/// These endpoints exist solely to verify [Authorize(Roles = "...")] works correctly.
/// Returns 200 for the correct role, 403 for wrong role, 401 for missing/invalid JWT.
/// </summary>
[ApiController]
[Route("api/test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public IActionResult AdminOnly() =>
        Ok(new { message = "You have ADMIN access." });

    [HttpGet("teacher")]
    [Authorize(Roles = "TEACHER")]
    public IActionResult TeacherOnly() =>
        Ok(new { message = "You have TEACHER access." });

    [HttpGet("student")]
    [Authorize(Roles = "STUDENT")]
    public IActionResult StudentOnly() =>
        Ok(new { message = "You have STUDENT access." });

    [HttpGet("admin-or-teacher")]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public IActionResult AdminOrTeacher() =>
        Ok(new { message = "You have ADMIN or TEACHER access." });

    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult AnyAuthenticatedUser() =>
        Ok(new { message = "You are authenticated." });
}
