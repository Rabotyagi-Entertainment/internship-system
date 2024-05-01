using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.Internship.Requests;

public record InternshipCompanyDto(
    Guid StudentId,
    Guid CompanyId,
    int Priority,
    ProgressStatus? Status,
    string? AdditionalInfo);