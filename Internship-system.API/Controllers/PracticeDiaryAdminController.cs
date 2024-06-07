using Internship_system.BLL.DTOs.PracticeDiaryAdmin;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("admin/diary")]
public class PracticeDiaryAdminController : ControllerBase
{
    private readonly PracticeDiaryAdminService _diaryAdminService;

    public PracticeDiaryAdminController(PracticeDiaryAdminService diaryAdminService)
    {
        _diaryAdminService = diaryAdminService;
    }

    /// <summary>
    /// Export students as excel table with current internship 
    /// </summary>
    [HttpGet]
    [Route("students/table")]
    public async Task<IActionResult> ExportStudentsWithInternshipsAsTable()
    {
        var result = await _diaryAdminService.ExportStudentsInternshipsAsTable();
        result.Position = 0;
        return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exportStudentsInternships.xlsx");
    }

    /// <summary>
    /// Get list of students 
    /// </summary>
    [HttpGet]
    [Route("students")]
    public async Task<List<StudentListElemDto>> GetStudentsList([FromQuery] string fullName)
    {
        return await _diaryAdminService.GetStudentsList(fullName);
    }
    
    /// <summary>
    /// Get state of practice diary by id
    /// </summary>
    [HttpGet]
    [Route("diary/{diaryId}/status")]
    public async Task<DiaryState> GetStudentPracticeDiaryState(Guid diaryId)
    {
        return await _diaryAdminService.GetStudentDiaryState(diaryId);
    }

    /// <summary>
    /// Change practice diary state
    /// </summary>
    [HttpPut]
    [Route("diary/{diaryId}/status")]
    public async Task ChangeDiaryStatus([FromBody] DiaryState diaryState, Guid diaryId)
    {
        await _diaryAdminService.ChangeDiaryStatus(diaryState, diaryId);
    }

    /// <summary>
    /// Leave a comment for practice diary by id
    /// </summary>
    [HttpPost]
    [Route("diary/{diaryId}/comment")]
    public async Task LeavePracticeDiaryComment(Guid diaryId)
    {
        throw new NotImplementedException();
    }
}