using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.DAL.Data.Entities;

namespace Internship_system.BLL.Extensions;

public static class InternshipExtensions {
    public static InternshipDto ToFullDto(this Internship internship) {
        return new() {
            Id = internship.Id,
            Company = new(internship.Company.Id, internship.Company.Name, internship.Company.IsPartner),
            PracticeDiaries = internship.PracticeDiaries.Select(d => new PracticeDiaryDto {
                Id = d.Id,
                DiaryType = d.DiaryType,
                DiaryState = d.DiaryState,
                CreatedAt = d.CreatedAt,
                StudentFullName = internship.Student.FullName,
                CuratorFullName = d.CuratorFullName,
                TaskReportTable = d.TaskReportTable,
                StudentCharacteristics = d.StudentCharacteristics,
                CompanyName = d.Internship.Company.Name,
                WorkName = d.WorkName,
                PlanTable = d.PlanTable,
                Comments = d.Comments
                    .Select(c => new CommentDto {
                        Text = c.Text,
                        Author = c.User.FullName,
                        RoleType = c.RoleType
                    }).ToList()
            }).ToList(),
            StartedAt = internship.StartedAt,
            EndedAt = internship.EndedAt
        };
    }
}