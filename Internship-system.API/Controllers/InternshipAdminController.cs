using System.Security.Claims;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
/*[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "Dean")]*/
[Route("admin/internship")]
public class InternshipAdminController : ControllerBase
{
    private readonly InternshipAdminService _internshipAdminService;

    public InternshipAdminController(InternshipAdminService internshipAdminService)
    {
        _internshipAdminService = internshipAdminService;
    }

    /// <summary>
    /// Create new internship company for students
    /// </summary>
    [HttpPost]
    [Route("company")]
    public async Task CreateInternshipCompany([FromBody] CreateCompanyDto createCompanyDto)
    {
        await _internshipAdminService.CreateInternshipCompany(createCompanyDto);
    }

    /// <summary>
    /// Upload students list as table 
    /// </summary>
    [HttpPost]
    [Route("students")]
    [Obsolete]

    public async Task UploadStudents(IFormFile studentsTable)
    {
        await _internshipAdminService.UploadStudents(studentsTable);
    }

    /// <summary>
    /// Export students list as table
    /// </summary>
    [HttpGet]
    [Route("students/table")]
    public async Task<IActionResult> ExportStudentsAsTable()
    {
        var result = await _internshipAdminService.ExportStudentsAsTable();
        result.Position = 0;
        return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exportStudents.xlsx");
    }
    
    /// <summary>
    /// Get student companies list with statuses 
    /// </summary>
    [HttpGet]
    [Route("students/{userId}")]
    public async Task<List<StudentStatusDto>> GetStudentCompanies(Guid userId)
    {
        return await _internshipAdminService.GetStudentCompanies(userId);
    }

    /// <summary>
    /// Get students list with parameters 
    /// </summary>
    [HttpGet]
    [Route("students")]
    public async Task<List<StudentInfoDto>> GetStudentsList([FromQuery] StudentsQueryModel query)
    {
        return await _internshipAdminService.GetStudentsList(query);
    }

    /// <summary>
    /// Leave comment on internship progress
    /// </summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("{internshipProgressId}")]
    public async Task CommentStudentInternshipProgress([FromBody] string comment, Guid internshipProgressId)
    {
        var userId = Guid.Parse(User.Identity.Name);
        await _internshipAdminService.CommentInternshipProgress(comment, internshipProgressId, userId);
    }
}