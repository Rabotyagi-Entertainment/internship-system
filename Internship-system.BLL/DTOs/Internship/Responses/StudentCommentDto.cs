using internship_system.Common.Enums;
using Internship_system.DAL.Data.Entities;

namespace Internship_system.BLL.DTOs.Internship.Responses;

public record StudentCommentDto(
    Guid? InternshipProgressId,
    Guid? PracticeDiaryId,
    Guid UserId,
    RoleType RoleType,
    string Text
);