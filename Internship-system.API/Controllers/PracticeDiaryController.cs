using Internship_system.BLL.DTOs.PracticeDiary;
using internship_system.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("diary")]
public class PracticeDiaryController: Controller {
    
    
    /// <summary>
    /// Get my diaries
    /// </summary>
    [HttpGet]
    [Route("list")]
    public async Task<PracticeDiaryDto> GetDiaries() {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Get diary as file
    /// </summary>
    [HttpGet]
    [Route("{diaryId}")]
    public async Task<IFormFile> GetDiaryFile() {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Create practice diary (choose diary template)
    /// </summary>
    [HttpPost]
    [Route("{internshipId}/template")]
    public async Task CreateDiary(Guid internshipId, PracticeDiaryType diaryType) {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Edit general information in diary
    /// </summary>
    [HttpPost]
    [Route("{diaryId}")]
    public async Task EditGeneralInfo(Guid diaryId, EditGeneralInfoDto dto) {
        throw new NotImplementedException();
    } 
    /// <summary>
    /// Edit additional information in diary
    /// </summary>
    [HttpPost]
    [Route("{diaryId}/additional-info")]
    public async Task EditGeneralInfo(Guid diaryId, EditAdditionalInfoDto dto) {
        throw new NotImplementedException();
    } 
    
    /// <summary>
    /// Load task report xls file
    /// </summary>
    [HttpPost]
    [Route("{diaryId}/xls-file")]
    public async Task LoadXlsFile(Guid diaryId, IFormFile file) {
        throw new NotImplementedException();
    }
}