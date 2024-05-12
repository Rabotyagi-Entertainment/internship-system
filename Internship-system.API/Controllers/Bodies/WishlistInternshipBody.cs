using Internship_system.BLL.DTOs.Internship.Requests;
using internship_system.Common.Enums;

namespace Internship_system.Controllers.Bodies;

public record WishlistInternshipBody(
    int Priority,
    ProgressStatus? Status,
    string? AdditionalInfo) {
    public InternshipCompanyDto ToRequest(Guid studentId, Guid companyId) => new(studentId, companyId, Priority, Status, AdditionalInfo);
}