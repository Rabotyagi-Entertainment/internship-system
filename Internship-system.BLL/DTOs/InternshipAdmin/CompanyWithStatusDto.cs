using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public record CompanyWithStatusDto(Guid Id, string Name, ProgressStatus Status);