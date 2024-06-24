using Internship_system.BLL.DTOs.Internship.Responses;
using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public class StudentInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public CurrentCompany? CurrentCompany { get; set; }
    public List<CompanyWithStatusDto> Companies { get; set; } = new();
}