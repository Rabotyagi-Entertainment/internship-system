using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("diary")]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class PracticeDiaryController: Controller {
    private readonly PracticeDiaryService _practiceDiaryService;

    public PracticeDiaryController(PracticeDiaryService practiceDiaryService) {
        _practiceDiaryService = practiceDiaryService;
    }

    /// <summary>
    /// Get my diaries
    /// </summary>
    [HttpGet]
    [Route("list")]
    public async Task<List<PracticeDiaryDto>> GetDiaries() {
        var userId = Guid.Parse(User.Identity.Name);
        return await _practiceDiaryService.GetDiaries(userId);
    }
    /// <summary>
    /// Get diary as file
    /// </summary>
    [HttpGet]
    [Route("{diaryId}")]
    public IActionResult GetDiaryFile(Guid diaryId) {
        var memoryStream =  _practiceDiaryService.GetDiaryFile(diaryId).Result;
        return File(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document","exmaple.docx");
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
    /// Edit general information in diary
    /// </summary>
    [HttpPut]
    [Route("{diaryId}")]
    public async Task EditGeneralInfo(Guid diaryId, EditGeneralInfoDto dto) {
        await _practiceDiaryService.EditGeneralInfo(diaryId, dto);
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