using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.Services;
using internship_system.Common.Enums;
using Internship_system.Controllers.Bodies;
using Internship_system.Controllers.Extensions;
using Internship_system.DAL.Data.Entities;
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
    public async Task<ActionResult<Guid>> AddCompany(Guid companyId, [FromBody] WishlistInternshipBody body) {
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
    public async Task<IActionResult> DeleteCompany(Guid companyId) {
        var userId = this.GetUserId();
        await _internshipService.DeleteDesiredCompanyFromProgress(userId, companyId);
        return NoContent();
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
    public async Task<IActionResult> ChangeCompanyStatus(Guid companyId, [FromBody] ProgressStatus status) {
        var userId = this.GetUserId();
        await _internshipService.UpdateCompanyStatus(new(userId, companyId, status));
        return NoContent();
    }

    /// <summary>
    /// Оставить комментарий к статусу
    /// </summary>
    [HttpPost]
    [Route("progress/{internshipProgressId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> LeaveStatusComment(Guid internshipProgressId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var result = await _internshipService.StudentLeaveProgressComment(new(internshipProgressId, userId, body.Text));
        return Ok(result);
    }

    /// <summary>
    /// Оставить комментарий к компании
    /// </summary>
    [HttpPost]
    [Route("progress/diary/{practiceDiaryId:guid}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> LeaveCompanyComment(Guid practiceDiaryId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var dto = new StudentLeaveDiaryCommentDto(practiceDiaryId, userId, body.Text);
        var result = await _internshipService.StudentLeavePracticeDiaryComment(dto);
        return Ok(result);
    }

    /// <summary>
    /// Оставить комментарий к компании
    /// </summary>
    [HttpPatch]
    [Route("progress/{internshipProgressId:guid}/update")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> ChangeInternshipStatus(Guid internshipProgressId, [FromBody] UpdateInternshipProgressBody body) {
        var userId = this.GetUserId();
        var dto = new UpdateInternshipProgressDto(internshipProgressId, userId, body.Priority, body.AdditionalInfo);
        var result = await _internshipService.UpdateInternshipProgress(dto);
        return Ok(result);
    }

    // Отправка сообщений в бота, как я понимаю, будет происходить автоматически на бэке

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