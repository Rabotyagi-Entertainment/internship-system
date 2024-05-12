using Internship_system.BLL.DTOs.Internship;
using Internship_system.BLL.DTOs.Internship.Requests;
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

    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/{companyId:guid}")]
    [EndpointSummary("Добавление желаемых компаний из списка партнеров")]
    public async Task<IActionResult> AddCompany(Guid companyId, [FromBody] WishlistInternshipBody body) {
        var userId = this.GetUserId();
        var response = await _internshipService.AddDesiredCompanyToInternship(body.ToRequest(userId, companyId));
        return Ok(response);
    }

    [HttpPost]
    [Route("company/create")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Создание компании студентом, если её нет в списке компаний-партнёров")]
    public async Task<IActionResult> CreateNonPartnerCompany([FromBody] string name) {
        var userId = this.GetUserId();
        var response = await _internshipService.CreateNonPartnerCompany(new(userId, name));
        return Ok(response);
    }

    [HttpPut]
    [Route("company/{companyId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Изменение статуса компании, выбранной студентом")]
    public async Task<IActionResult> ChangeCompanyStatus(Guid companyId, [FromBody] ProgressStatus status) {
        var userId = this.GetUserId();
        await _internshipService.UpdateCompanyStatus(new(userId, companyId, status));
        return NoContent();
    }

    [HttpPost]
    [Route("progress/{internshipProgressId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Оставить комментарий к статусу")]
    public async Task<IActionResult> LeaveStatusComment(Guid internshipProgressId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var result = await _internshipService.StudentLeaveProgressComment(new(internshipProgressId, userId, body.Text));
        return Ok(result);
    }

    [HttpPost]
    [Route("progress/{internshipProgressId:guid}/diary")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Оставить комментарий к компании")]
    public async Task<IActionResult> LeaveCompanyComment(Guid internshipProgressId, [FromBody] StudentLeaveProgressCommentBody body) {
        var userId = this.GetUserId();
        var dto = new StudentLeaveDiaryCommentDto(internshipProgressId, userId, body.Text);
        var result = await _internshipService.StudentLeavePracticeDiaryComment(dto);
        return Ok(result);
    }
    // Отправка сообщений в бота, как я понимаю, будет происходить автоматически на бэке
}