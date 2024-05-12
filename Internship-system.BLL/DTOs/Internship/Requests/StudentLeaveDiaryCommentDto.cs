namespace Internship_system.BLL.DTOs.Internship.Requests;

public record StudentLeaveDiaryCommentDto(Guid PracticeDiaryId, Guid StudentId, string Text);