using Internship_system.BLL.DTOs.Internship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("internship")]
public class InternshipController : Controller
{
    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("company/{companyId:guid}")]
    [EndpointSummary("")]
    public IActionResult AddCompany(Guid companyId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("company/create")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EndpointSummary("Создание компании студентом, если её нет в списке компаний")]
    public IActionResult CreateCustomCompany([FromBody] CreateCustomCompanyDto body)
    {
        throw new NotImplementedException();
    }

    [HttpPut]
    [Route("company/{companyId:guid}/status")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult ChangeCompanyStatus(Guid companyId, [FromBody] ChangeCompanyStatusDto body)
    {
        throw new NotImplementedException();
    }
    // Отправка сообщений в бота, как я понимаю, будет происходить автоматически на бэке
}