namespace Internship_system.BLL.DTOs.Internship.Requests;

public record StudentLeaveProgressCommentDto(Guid StudentId, Guid CompanyId, string Text);