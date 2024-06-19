using internship_system.Common.Enums;

namespace Internship_system.Controllers.Bodies;

public record UpdateInternshipProgressBody(int? Priority = null, ProgressStatus? Status = null, string? AdditionalInfo = null);

public record UpdateInternshipProgressWithCompanyIdBody(Guid CompanyId, int? Priority = null, ProgressStatus? Status = null, string? AdditionalInfo = null);