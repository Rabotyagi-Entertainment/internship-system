using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.Internship;

public class ChangeCompanyStatusDto
{
    public required ProgressStatus Status { get; set; }
}