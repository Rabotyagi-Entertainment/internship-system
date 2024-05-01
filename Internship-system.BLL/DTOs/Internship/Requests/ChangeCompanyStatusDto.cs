using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.Internship.Requests;

public record ChangeCompanyStatusDto(Guid StudentId, ProgressStatus Status);