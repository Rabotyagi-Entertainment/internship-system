using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommentDto = Internship_system.BLL.DTOs.InternshipAdmin.CommentDto;

namespace Internship_system.BLL.Services;

public class InternshipService {
    private readonly InterDbContext _dbContext;

    public InternshipService(InterDbContext dbContext) {
        _dbContext = dbContext;
    }

    // Где-то выше (или здесь) должна быть проверка на то, что студент - именно студент второго курса 
    public async Task<InternshipProgress> AddDesiredCompanyToInternship(InternshipCompanyDto dto) {
        var student = await _dbContext.Students.FindAsync(dto.StudentId) ??
                      throw new NotFoundException($"User with id {dto.StudentId} not found");
        var company = await _dbContext.Companies.FindAsync(dto.CompanyId) ??
                      throw new NotFoundException($"Company with id {dto.CompanyId} not found");

        var internshipProgress = new InternshipProgress {
            Student = student,
            Company = company,
            Priority = dto.Priority,
            ProgressStatus = dto.Status ?? ProgressStatus.Default,
            AdditionalInfo = dto.AdditionalInfo
        };
        _dbContext.Add(internshipProgress);
        await _dbContext.SaveChangesAsync();

        return internshipProgress;
    }

    public async Task<CompanyResponseDto> CreateNonPartnerCompany(CreateCustomCompanyDto request) {
        var company = new Company {
            Name = request.Name,
            IsPartner = false
        };
        _dbContext.Add(company);
        await _dbContext.SaveChangesAsync();

        return new(company.Id, company.Name, company.IsPartner ?? false);
    }

    public async Task UpdateCompanyStatus(UpdateCompanyStatusDto dto) {
        var internshipProgress = await _dbContext.InternshipProgresses
                                     .Where(ip => ip.Student.Id == dto.StudentId && ip.Company.Id == dto.CompanyId)
                                     .FirstOrDefaultAsync()
                                 ?? throw new NotFoundException(
                                     $"Internship Progresses for company id '{dto.CompanyId}' and user id '{dto.StudentId}' not found");

        internshipProgress.ProgressStatus = dto.NewStatus;
        _dbContext.Update(internshipProgress);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StudentCommentDto> StudentLeaveProgressComment(StudentLeaveProgressCommentDto dto) {
        var internshipProgress = await _dbContext.InternshipProgresses.FindAsync(dto.InternshipProgressId)
                                 ?? throw new NotFoundException(
                                     $"Internship Progresses with id {dto.InternshipProgressId} not found");
        var student = await _dbContext.Students.FindAsync(dto.StudentId) ??
                      throw new NotFoundException($"User with id {dto.StudentId} not found");

        var comment = new Comment {
            InternshipProgress = internshipProgress,
            PracticeDiary = null,
            User = student,
            RoleType = RoleType.Student,
            Text = dto.Text
        };
        _dbContext.Add(comment);
        await _dbContext.SaveChangesAsync();

        return new(internshipProgress.Id, null, student.Id, comment.RoleType, comment.Text);
    }

    public async Task<StudentCommentDto> StudentLeavePracticeDiaryComment(StudentLeaveDiaryCommentDto dto) {
        var practiceDiary = await _dbContext.PracticeDiaries.FindAsync(dto.PracticeDiaryId)
                            ?? throw new NotFoundException($"Practice Diary with id {dto.PracticeDiaryId} not found");
        var student = await _dbContext.Students.FindAsync(dto.StudentId) ??
                      throw new NotFoundException($"User with id {dto.StudentId} not found");

        var comment = new Comment {
            InternshipProgress = null,
            PracticeDiary = practiceDiary,
            User = student,
            RoleType = RoleType.Student,
            Text = dto.Text
        };
        _dbContext.Add(comment);
        await _dbContext.SaveChangesAsync();

        return new(null, practiceDiary.Id, student.Id, comment.RoleType, comment.Text);
    }
}