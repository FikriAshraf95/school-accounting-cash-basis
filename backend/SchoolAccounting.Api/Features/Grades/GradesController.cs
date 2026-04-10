using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Grades;

[ApiController]
[Route("api/v1/grades")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly GradeService _gradeService;

    public GradesController(GradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GradeResponse>>> GetGrades(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var result = await _gradeService.GetGradesAsync(page, perPage, sortBy, sortDesc);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GradeResponse>> GetGrade(int id)
    {
        var grade = await _gradeService.GetGradeAsync(id);
        return Ok(grade);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<GradeResponse>> CreateGrade(CreateGradeRequest request)
    {
        var grade = await _gradeService.CreateGradeAsync(request);
        return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, grade);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<GradeResponse>> UpdateGrade(int id, UpdateGradeRequest request)
    {
        var grade = await _gradeService.UpdateGradeAsync(id, request);
        return Ok(grade);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRole.Admin)]
    public async Task<ActionResult> DeleteGrade(int id)
    {
        await _gradeService.DeleteGradeAsync(id);
        return NoContent();
    }
}
