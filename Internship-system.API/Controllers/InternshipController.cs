using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Internship_system.Controllers.Bodies;
using Internship_system.Controllers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("internship")]
public class InternshipController : Controller {
    private readonly InternshipService _internshipService;

    public InternshipController(InternshipService internshipService) {
        _internshipService = internshipService;
    }

    /// <summary>
    /// Получить студентом все компании, которые он выбрал для отбора на стажировку 
    /// </summary>
    [HttpGet]
    [Route("company/student")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetStudentCompanies() {
        return Ok(await _internshipService.GetStudentCompanies(this.GetUserId()));
    }

    /// <summary>
    /// Добавление желаемых компаний из списка партнеров
    /// </summary>
    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/{companyId:guid}")]
    public async Task<ActionResult<Guid>> AddCompanyToProgress(Guid companyId, [FromBody] WishlistInternshipBody body) {
        var userId = this.GetUserId();
        var response = await _internshipService.AddDesiredCompanyToProgress(body.ToRequest(userId, companyId));
        return Ok(response);
    }

    /// <summary>
    /// Удаление нежеланных компаний из списка партнеров
    /// </summary>
    [HttpDelete]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/{companyId:guid}")]
    public async Task<IActionResult> DeleteCompanyFromProgress(Guid companyId) {
        var userId = this.GetUserId();
        await _internshipService.DeleteDesiredCompanyFromProgress(userId, companyId);
        return NoContent();
    }

    /// <summary>
    /// Изменение компаний по списку id компаний
    /// </summary>
    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/change")]
    public async Task<IActionResult> UpdateDesiredCompaniesProgress([FromBody] List<UpdateInternshipProgressWithCompanyIdBody> companiesUpdates) {
        var userId = this.GetUserId();
        var dtos = companiesUpdates
            .Select(body => new UpdateInternshipProgressDto(body.CompanyId, body.Priority, body.Status, body.AdditionalInfo))
            .ToList();
        var newProgresses = await _internshipService.UpdateDesiredCompaniesProgress(userId, dtos);
        return Ok(newProgresses);
    }

    /// <summary>
    /// Создание компании студентом, если её нет в списке компаний-партнёров
    /// </summary>
    [HttpPost]
    [Route("company/create")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> CreateNonPartnerCompany([FromBody] string name) {
        var userId = this.GetUserId();
        var response = await _internshipService.CreateNonPartnerCompany(new(userId, name));
        return Ok(response);
    }

    /// <summary>
    /// Изменение статуса компании для стажировки, выбранной студентом
    /// </summary>
    [HttpPut]
    [Route("company/{companyId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> ChangeCompanyStatus(Guid companyId, [FromQuery] ProgressStatus status) {
        var userId = this.GetUserId();
        await _internshipService.UpdateCompanyStatus(new(userId, companyId, status));
        return NoContent();
    }

    /// <summary>
    /// Оставить комментарий к статусу
    /// </summary>
    [HttpPost]
    [Route("progress/{companyId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> LeaveStatusComment(Guid companyId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var result = await _internshipService.StudentLeaveProgressComment(new(userId, companyId, body.Text));
        return Ok(result);
    }

    /// <summary>
    /// Оставить комментарий к дневнику практики
    /// </summary>
    [HttpPost]
    [Route("progress/diary/{practiceDiaryId:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> LeavePracticeDiaryComment(Guid practiceDiaryId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var dto = new StudentLeaveDiaryCommentDto(practiceDiaryId, userId, body.Text);
        var result = await _internshipService.StudentLeavePracticeDiaryComment(dto);
        return Ok(result);
    }

    /// <summary>
    /// Обновить информацию о прогрессе стажировки
    /// </summary>
    [HttpPatch]
    [Route("progress/{companyId:guid}/update")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> ChangeInternshipStatus(Guid companyId, [FromBody] UpdateInternshipProgressBody body) {
        var userId = this.GetUserId();
        var dto = new UpdateInternshipProgressDto(companyId, body.Priority, body.Status, body.AdditionalInfo);
        var result = await _internshipService.UpdateInternshipProgress(userId, dto);
        return Ok(result);
    }

    /// <summary>
    /// Получить студентом все его InternshipProgress 
    /// </summary>
    [HttpGet]
    [Route("progress/student")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<List<InternshipDto>>> GetStudentInternshipProgresses() {
        return Ok(await _internshipService.GetStudentInternshipProgresses(this.GetUserId()));
    }

    /// <summary>
    /// Получить студентом все его Internship 
    /// </summary>
    [HttpGet]
    [Route("internship/student")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<List<InternshipDto>>> GetStudentInternships() {
        return Ok(await _internshipService.GetStudentInternships(this.GetUserId()));
    }
}