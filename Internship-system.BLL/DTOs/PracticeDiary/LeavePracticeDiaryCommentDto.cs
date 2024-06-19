namespace Internship_system.BLL.DTOs.PracticeDiary;

public record LeavePracticeDiaryCommentDto(Guid UserId, Guid DiaryId, string Text);