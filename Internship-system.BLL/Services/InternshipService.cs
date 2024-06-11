using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Internship_system.BLL.Services;

public class InternshipService {
    private readonly InterDbContext _dbContext;

    public InternshipService(InterDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<List<InternshipProgressDto>> GetStudentInternshipProgresses(Guid studentId) {
        var student = await _dbContext.Students.FindAsync(studentId) ??
                      throw new NotFoundException($"User with id {studentId} not found");
        var internshipProgresses =  await _dbContext.InternshipProgresses
            .Include(progress => progress.Student)
            .Include(progress => progress.Company)
            .Include(progress => progress.Comments)
            .ThenInclude(comment => comment.User)
            .Where(progress => progress.Student == student)
            .ToListAsync();
        return internshipProgresses
            .Select(ip => new InternshipProgressDto {
                Id = ip.Id,
                Company = new CompanyDto(ip.Company.Id, ip.Company.Name, ip.Company.IsPartner),
                Priority = ip.Priority,
                ProgressStatus = ip.ProgressStatus,
                AdditionalInfo = ip.AdditionalInfo,
                CreatedAt = ip.CreatedAt,
                EditedAt = ip.EditedAt,
                Comments = ip.Comments.Select(c=> new CommentDto {
                    Text = c.Text,
                    Author = c.User.FullName,
                    RoleType = c.RoleType
                }).ToList()
            }).ToList();
    }

    public async Task<List<InternshipDto>> GetStudentInternships(Guid studentId) {
        var student = await _dbContext.Students.FindAsync(studentId) ??
                      throw new NotFoundException($"User with id {studentId} not found");
        
        var internships =  await _dbContext.Internships
            .Include(internship => internship.Student)
            .Include(internship => internship.Company)
            .Include(internship => internship.PracticeDiaries)
            .ThenInclude(practiceDiary => practiceDiary.Comments)
            .ThenInclude(comment => comment.User)
            .Where(internship => internship.Student == student)
            .ToListAsync();
        return internships
            .Select(i => new InternshipDto {
                Id = i.Id,
                Company = new CompanyDto(i.Company.Id, i.Company.Name, i.Company.IsPartner),
                PracticeDiaries = i.PracticeDiaries.Select(d => new PracticeDiaryDto {
                    Id = d.Id,
                    DiaryType = d.DiaryType,
                    DiaryState = d.DiaryState,
                    CreatedAt = d.CreatedAt,
                    StudentFullName = i?.Student.FullName,
                    CuratorFullName = d.CuratorFullName,
                    TaskReportTable = d.TaskReportTable,
                    StudentCharacteristics = d.StudentCharacteristics,
                    CompanyName = d.Internship.Company.Name,
                    WorkName = d.WorkName,
                    PlanTable = d.PlanTable,
                    Comments = d.Comments
                        .Select(c=> new CommentDto {
                            Text = c.Text,
                            Author = c.User.FullName,
                            RoleType = c.RoleType
                        }).ToList()
                }).ToList(),
                StartedAt = i.StartedAt,
                EndedAt = i.EndedAt
            }).ToList();
    }

    public async Task<List<Company>> GetStudentCompanies(Guid studentId) {
        return await _dbContext.InternshipProgresses
            .Include(progress => progress.Company)
            .Where(progress => progress.Student.Id == studentId)
            .Select(progress => progress.Company)
            .ToListAsync();
    }

    // Где-то выше (или здесь) должна быть проверка на то, что студент - именно студент второго курса 
    public async Task<Guid> AddDesiredCompanyToInternship(InternshipCompanyDto dto) {
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

        return internshipProgress.Id;
    }

    public async Task<CompanyDto> CreateNonPartnerCompany(CreateCustomCompanyDto request) {
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

        if (dto.NewStatus == ProgressStatus.AcceptedOffer) {
            var student = await _dbContext.Students.FindAsync(dto.StudentId) ??
                          throw new NotFoundException($"User with id {dto.StudentId} not found");
            var company = await _dbContext.Companies.FindAsync(dto.CompanyId) ??
                          throw new NotFoundException($"Company with id {dto.CompanyId} not found");
            _dbContext.Add(new Internship {
                Student = student,
                Company = company,
                PracticeDiaries = [],
                EndedAt = null
            });
        }

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

    public async Task<InternshipProgress> UpdateInternshipProgress(UpdateInternshipProgressDto dto) {
        var internshipProgress = await _dbContext.InternshipProgresses.FindAsync(dto.InternshipProgressId)
                                 ?? throw new NotFoundException($"Internship Progress with id {dto.InternshipProgressId} not found");

        if (dto.Priority == null && dto.AdditionalInfo == null) throw new BadRequestException("There must be at least one change");

        internshipProgress.Priority = dto.Priority ?? internshipProgress.Priority;
        internshipProgress.AdditionalInfo = dto.AdditionalInfo ?? internshipProgress.AdditionalInfo;
        internshipProgress.EditedAt = DateTime.UtcNow;

        _dbContext.Update(internshipProgress);

        return internshipProgress;
    }
}