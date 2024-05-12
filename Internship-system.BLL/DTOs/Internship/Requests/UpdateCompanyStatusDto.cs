using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.Internship.Requests;

public record UpdateCompanyStatusDto(Guid StudentId, Guid CompanyId, ProgressStatus NewStatus);