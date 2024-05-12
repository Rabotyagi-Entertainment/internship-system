namespace Internship_system.BLL.DTOs.Internship.Requests;

public record StudentLeaveProgressCommentDto(Guid InternshipProgressId, Guid StudentId, string Text);