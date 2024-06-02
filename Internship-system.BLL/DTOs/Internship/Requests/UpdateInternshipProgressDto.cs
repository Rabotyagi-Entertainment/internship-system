namespace Internship_system.BLL.DTOs.Internship.Requests;

public record UpdateInternshipProgressDto(Guid StudentId, Guid InternshipProgressId, int? Priority = null, string? AdditionalInfo = null);