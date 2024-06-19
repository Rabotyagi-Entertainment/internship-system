using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Internship_system.BLL.Extensions;

public static class InternshipProgressesExtensions {
    public static InternshipProgressDto ToFullDto(this InternshipProgress progress) {
        return new() {
            Id = progress.Id,
            Company = new(progress.Company.Id, progress.Company.Name, progress.Company.IsPartner),
            Priority = progress.Priority,
            ProgressStatus = progress.ProgressStatus,
            AdditionalInfo = progress.AdditionalInfo,
            CreatedAt = progress.CreatedAt,
            EditedAt = progress.EditedAt,
            Comments = progress.Comments.Select(c => new CommentDto {
                Text = c.Text,
                Author = c.User.FullName,
                RoleType = c.RoleType
            }).ToList()
        };
    }

    public static InternshipProgress CreateAndAttachDefaultProgress(
        this DbSet<InternshipProgress> dbSet,
        Student student,
        Company company,
        UpdateInternshipProgressDto dto
    ) {
        var progress = new InternshipProgress {
            Student = student,
            Company = company,
            Priority = dto.Priority,
            ProgressStatus = dto.Status ?? ProgressStatus.Default,
            AdditionalInfo = dto.AdditionalInfo
        };
        dbSet.Add(progress);

        return progress;
    }

    public static async Task<InternshipProgress> GetProgressOrThrow(this DbSet<InternshipProgress> dbSet, Guid studentId, Guid companyId) {
        return await dbSet
                   .Where(ip => ip.Student.Id == studentId && ip.Company.Id == companyId)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException($"Internship Progresses for company id '{companyId}' and user id '{studentId}' not found");
    }

    public static async Task<InternshipProgress> GetProgressWithAllInclusionsOrThrow(this DbSet<InternshipProgress> dbSet, Guid studentId, Guid companyId) {
        return await dbSet
                   .Include(progress => progress.Student)
                   .Include(progress => progress.Company)
                   .Include(progress => progress.Comments)
                   .ThenInclude(comment => comment.User)
                   .Where(ip => ip.Student.Id == studentId && ip.Company.Id == companyId)
                   .FirstOrDefaultAsync()
               ?? throw new NotFoundException($"Internship Progresses for company id '{companyId}' and user id '{studentId}' not found");
    }

    public static async Task RemoveProgress(this DbSet<InternshipProgress> dbSet, Guid studentId, Guid companyId) {
        await dbSet.Where(progress => progress.Student.Id == studentId && progress.Company.Id == companyId).ExecuteDeleteAsync();
    }
}