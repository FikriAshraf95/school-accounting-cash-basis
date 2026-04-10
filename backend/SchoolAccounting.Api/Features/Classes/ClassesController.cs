using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Classes;

[ApiController]
[Route("api/v1/classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly ClassService _classService;

    public ClassesController(ClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClassResponse>>> GetClasses(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var result = await _classService.GetClassesAsync(page, perPage, sortBy, sortDesc);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClassDetailResponse>> GetClass(int id)
    {
        var classEntity = await _classService.GetClassAsync(id);
        return Ok(classEntity);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<ClassResponse>> CreateClass(CreateClassRequest request)
    {
        var classEntity = await _classService.CreateClassAsync(request);
        return CreatedAtAction(nameof(GetClass), new { id = classEntity.Id }, classEntity);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<ClassResponse>> UpdateClass(int id, UpdateClassRequest request)
    {
        var classEntity = await _classService.UpdateClassAsync(id, request);
        return Ok(classEntity);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRole.Admin)]
    public async Task<ActionResult> DeleteClass(int id)
    {
        await _classService.DeleteClassAsync(id);
        return NoContent();
    }

    [HttpPost("{classId}/students")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult> AssignStudent(int classId, AssignStudentRequest request)
    {
        await _classService.AssignStudentAsync(classId, request.StudentId);
        return NoContent();
    }

    [HttpDelete("{classId}/students/{studentId}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult> RemoveStudent(int classId, int studentId)
    {
        await _classService.RemoveStudentAsync(classId, studentId);
        return NoContent();
    }
}
