using Internship_system.BLL.DTOs.Internship.Requests;
using internship_system.Common.Enums;

namespace Internship_system.Controllers.Bodies;

public record WishlistInternshipBody(
    Guid StudentId,
    int Priority,
    ProgressStatus? Status,
    string? AdditionalInfo) {
    public InternshipCompanyDto ToRequest(Guid companyId) => new(StudentId, companyId, Priority, Status, AdditionalInfo);
}