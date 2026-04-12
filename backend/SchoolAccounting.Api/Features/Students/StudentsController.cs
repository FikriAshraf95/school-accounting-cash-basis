using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Students;

[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly StudentService _studentService;

    public StudentsController(StudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentResponse>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] string? search = null,
        [FromQuery] int? classId = null,
        [FromQuery] int? gradeId = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _studentService.GetStudentsAsync(page, perPage, sortBy, sortDesc, search, classId, gradeId, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDetailResponse>> GetStudent(int id)
    {
        var student = await _studentService.GetStudentAsync(id);
        return Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<StudentResponse>> CreateStudent(CreateStudentRequest request)
    {
        var student = await _studentService.CreateStudentAsync(request);
        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<StudentResponse>> UpdateStudent(int id, UpdateStudentRequest request)
    {
        var student = await _studentService.UpdateStudentAsync(id, request);
        return Ok(student);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRole.Admin)]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        await _studentService.DeleteStudentAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/assign-class")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    public async Task<ActionResult<StudentResponse>> AssignToClass(int id, AssignStudentToClassRequest request)
    {
        var student = await _studentService.AssignToClassAsync(id, request.ClassId);
        return Ok(student);
    }

    [HttpGet("report")]
    public async Task<ActionResult<List<StudentReportResponse>>> GetStudentReport(
        [FromQuery] int? gradeId = null,
        [FromQuery] int? classId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var request = new StudentReportRequest
        {
            GradeId = gradeId,
            ClassId = classId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        var result = await _studentService.GetStudentReportAsync(request);
        return Ok(result);
    }

    [HttpPost("import")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Staff}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StudentImportResult>> ImportStudents(IFormFile file)
    {
        // Parse CSV file
        var students = new List<CreateStudentRequest>();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            // Skip header
            await reader.ReadLineAsync();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    var student = new CreateStudentRequest
                    {
                        StudentId = parts[0].Trim(),
                        Name = parts[1].Trim()
                    };

                    if (parts.Length > 2 && int.TryParse(parts[2].Trim(), out var classId))
                        student.ClassId = classId;

                    if (parts.Length > 3 && int.TryParse(parts[3].Trim(), out var gradeId))
                        student.GradeId = gradeId;

                    if (parts.Length > 4)
                        student.Email = parts[4].Trim();

                    if (parts.Length > 5)
                        student.Phone = parts[5].Trim();

                    if (parts.Length > 6)
                        student.Address = parts[6].Trim();

                    if (parts.Length > 7 && bool.TryParse(parts[7].Trim(), out var isActive))
                        student.IsActive = isActive;

                    students.Add(student);
                }
            }
        }

        var result = await _studentService.ImportStudentsAsync(students);
        return Ok(result);
    }
}
