using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.DTOs.PracticeDiaryAdmin;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Internship_system.Controllers.Bodies;
using Internship_system.Controllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("admin/diary")]
public class PracticeDiaryAdminController : Controller {
    private readonly PracticeDiaryAdminService _diaryAdminService;

    public PracticeDiaryAdminController(PracticeDiaryAdminService diaryAdminService) {
        _diaryAdminService = diaryAdminService;
    }

    /// <summary>
    /// Export students as excel table with current internship 
    /// </summary>
    [HttpGet]
    [Route("students/table")]
    public async Task<IActionResult> ExportStudentsWithInternshipsAsTable() {
        var result = await _diaryAdminService.ExportStudentsInternshipsAsTable();
        result.Position = 0;
        return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exportStudentsInternships.xlsx");
    }

    /// <summary>
    /// Get list of students 
    /// </summary>
    [HttpGet]
    [Route("students")]
    public async Task<List<StudentListElemDto>> GetStudentsList([FromQuery] string fullName) {
        return await _diaryAdminService.GetStudentsList(fullName);
    }

    /// <summary>
    /// Get state of practice diary by id
    /// </summary>
    [HttpGet]
    [Route("{diaryId:guid}/status")]
    public async Task<DiaryState> GetStudentPracticeDiaryState(Guid diaryId) {
        return await _diaryAdminService.GetStudentDiaryState(diaryId);
    }

    /// <summary>
    /// Change practice diary state
    /// </summary>
    [HttpPut]
    [Route("{diaryId:guid}/status")]
    public async Task ChangeDiaryStatus([FromBody] DiaryState diaryState, Guid diaryId) {
        await _diaryAdminService.ChangeDiaryStatus(diaryState, diaryId);
    }

    /// <summary>
    /// Leave a comment for practice diary by id
    /// </summary>
    [HttpPost]
    [Route("{diaryId:guid}/comment")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<CommentDto>> LeavePracticeDiaryComment(Guid diaryId, [FromBody] LeavePracticeDiaryCommentBody body) {
        var userId = this.GetUserId();
        var dto = new LeavePracticeDiaryCommentDto(UserId: userId, DiaryId: diaryId, body.Text);
        var response = await _diaryAdminService.LeavePracticeDiaryComment(dto);
        return Ok(response);
    }

    /// <summary>
    /// Get diaries by internship id
    /// </summary>
    [HttpGet]
    [Route("internship/{internshipId:guid}")]
    public async Task<IActionResult> GetDiariesByInternshipId(Guid internshipId) {
        return Ok(await _diaryAdminService.GetPracticeDiariesByInternshipId(internshipId));
    }
}