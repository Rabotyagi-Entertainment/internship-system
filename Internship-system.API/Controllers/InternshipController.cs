using Internship_system.BLL.DTOs.Internship;
using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.Services;
using Internship_system.Controllers.Bodies;
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
        var response = await _internshipService.AddDesiredCompanyToInternship(body.ToRequest(companyId));
        return Ok(response);
    }

    [HttpPost]
    [Route("company/create")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Создание компании студентом, если её нет в списке компаний-партнёров")]
    public async Task<IActionResult> CreateNonPartnerCompany([FromBody] CreateCustomCompanyDto body) {
        var response = await _internshipService.CreateNonPartnerCompany(body);
        return Ok(response);
    }

    [HttpPut]
    [Route("company/{companyId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Изменение статуса компании, выбранной студентом")]
    public IActionResult ChangeCompanyStatus(Guid companyId, [FromBody] ChangeCompanyStatusDto body) {
        throw new NotImplementedException();
    }
    // Отправка сообщений в бота, как я понимаю, будет происходить автоматически на бэке
}