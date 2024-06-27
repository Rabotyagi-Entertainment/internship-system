using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Internship_system.Controllers.Bodies;
using Internship_system.Controllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("diary")]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class PracticeDiaryController : Controller {
    private readonly PracticeDiaryService _practiceDiaryService;

    public PracticeDiaryController(PracticeDiaryService practiceDiaryService) {
        _practiceDiaryService = practiceDiaryService;
    }

    /// <summary>
    /// Get diaries by params
    /// </summary>
    /// <remarks>
    /// userId == null значит берет всех, с internshipId так же
    /// </remarks>
    [HttpGet]
    [Route("list")]
    public async Task<List<PracticeDiaryDto>> GetDiaries(Guid? userId, Guid? internshipId) {
        return await _practiceDiaryService.GetDiaries(userId, internshipId);
    }

    /// <summary>
    /// Get diary as file
    /// </summary>
    [HttpGet]
    [Route("{diaryId}")]
    public IActionResult GetDiaryFile(Guid diaryId) {
        var (memoryStream, name) = _practiceDiaryService.GetDiaryFile(diaryId).Result;
        return File(memoryStream, 
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Дневник_практики_{name}.docx");
    }
    
    /// <summary>
    /// Get diaries as zip
    /// </summary>
    [HttpGet]
    [Route("students/course")]
    public IActionResult GetDiaryFile(int courseNumber) {
        var memoryStream = _practiceDiaryService.GetZipDiariesByCourseNumber(courseNumber).Result;
        memoryStream.Position = 0;
        return File(memoryStream, "application/zip", $"Дневники_{courseNumber}_курс.zip");
    }

    /// <summary>
    /// Send diary file to tg
    /// </summary>
    [HttpGet]
    [Route("{diaryId}/tg")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task GetDiaryFileTg(Guid diaryId) {
        var userId = this.GetUserId();
        await _practiceDiaryService.GetDiaryFileTg(diaryId, userId);
    }

    /// <summary>
    /// Create practice diary (choose diary template)
    /// </summary>
    [HttpPost]
    [Route("{internshipId}/template")]
    public async Task CreateDiary(Guid internshipId, PracticeDiaryType diaryType) {
        await _practiceDiaryService.CreateDiary(internshipId, diaryType);
    } 
    /// <summary>
    /// Delete practise diary
    /// </summary>
    [HttpDelete]
    [Route("{diaryId}")]
    public async Task DeleteDiary(Guid diaryId) {
        await _practiceDiaryService.DeleteDiary(diaryId);
    }

    /// <summary>
    /// Edit general information in diary
    /// </summary>
    [HttpPut]
    [Route("{diaryId}")]
    public async Task EditGeneralInfo(Guid diaryId, EditGeneralInfoDto dto) {
        await _practiceDiaryService.EditGeneralInfo(diaryId, dto);
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
        var response = await _practiceDiaryService.LeavePracticeDiaryComment(dto);
        return Ok(response);
    }
    
    /// <summary>
    /// Edit additional information in diary
    /// </summary>
    [HttpPut]
    [Route("{diaryId}/additional-info")]
    public async Task EditAdditionalInfo(Guid diaryId, EditAdditionalInfoDto dto) {
        await _practiceDiaryService.EditAdditionalInfo(diaryId, dto);
    }

    /// <summary>
    /// Load task report xls file
    /// </summary>
    [HttpPost]
    [Route("{diaryId}/xls-file")]
    public async Task LoadXlsFile(Guid diaryId, IFormFile file) {
        await _practiceDiaryService.LoadXlsFile(diaryId, file);
    }
}