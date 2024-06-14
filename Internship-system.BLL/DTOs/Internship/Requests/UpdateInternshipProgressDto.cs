using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.Internship.Requests;

public record UpdateInternshipProgressDto(Guid CompanyId, int? Priority = null, ProgressStatus? Status = null, string? AdditionalInfo = null);