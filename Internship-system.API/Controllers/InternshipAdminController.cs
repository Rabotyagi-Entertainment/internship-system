using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
/*[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "Dean")]*/
[Route("admin/internship")]
public class InternshipAdminController : ControllerBase {
    private readonly InternshipAdminService _internshipAdminService;
    private readonly InternshipService _internshipService;

    public InternshipAdminController(InternshipAdminService internshipAdminService, InternshipService internshipService) {
        _internshipAdminService = internshipAdminService;
        _internshipService = internshipService;
    }

    [HttpGet]
    [Route("company")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetAllCompanies() {
        return Ok(await _internshipAdminService.GetAllCompanies());
    }

    /// <summary>
    /// Create new internship company for students
    /// </summary>
    [HttpPost]
    [Route("company")]
    public async Task CreateInternshipCompany([FromBody] CreateCompanyDto createCompanyDto) {
        await _internshipAdminService.CreateInternshipCompany(createCompanyDto);
    }

    /// <summary>
    /// Upload students list as table 
    /// </summary>
    [HttpPost]
    [Route("students")]
    [Obsolete]
    public async Task UploadStudents(IFormFile studentsTable) {
        await _internshipAdminService.UploadStudents(studentsTable);
    }

    /// <summary>
    /// Export students list as table
    /// </summary>
    [HttpGet]
    [Route("students/table")]
    public async Task<IActionResult> ExportStudentsAsTable() {
        var result = await _internshipAdminService.ExportStudentsAsTable();
        result.Position = 0;
        return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exportStudents.xlsx");
    }

    /// <summary>
    /// Get student companies list with statuses 
    /// </summary>
    [HttpGet]
    [Route("students/{userId}")]
    public async Task<List<StudentStatusDto>> GetStudentCompanies(Guid userId) {
        return await _internshipAdminService.GetStudentCompanies(userId);
    }

    /// <summary>
    /// Get students list with parameters 
    /// </summary>
    [HttpGet]
    [Route("students")]
    public async Task<List<StudentInfoDto>> GetStudentsList([FromQuery] StudentsQueryModel query) {
        return await _internshipAdminService.GetStudentsList(query);
    }

    /// <summary>
    /// Leave comment on internship progress
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("progress/{internshipProgressId:guid}")]
    public async Task CommentStudentInternshipProgress([FromBody] string comment, Guid internshipProgressId) {
        var userId = Guid.Parse(User.Identity.Name);
        await _internshipAdminService.CommentInternshipProgress(comment, internshipProgressId, userId);
    }

    /// <summary>
    /// Get all the student companies that he has chosen
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentCompaniesOnly(Guid studentId) {
        return Ok(await _internshipAdminService.GetStudentCompaniesOnly(studentId));
    }

    /// <summary>
    /// Get all the student internships
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("internship/student/{studentId:guid}")]
    public async Task<ActionResult<List<InternshipDto>>> GetStudentInternships(Guid studentId) {
        return Ok(await _internshipService.GetStudentInternships(studentId));
    }

    /// <summary>
    /// Get all the student's internship progresses
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("progress/student/{studentId:guid}")]
    public async Task<ActionResult<List<InternshipDto>>> GetStudentInternshipProgresses(Guid studentId) {
        return Ok(await _internshipService.GetStudentInternshipProgresses(studentId));
    }
}