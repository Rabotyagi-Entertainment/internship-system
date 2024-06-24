using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.Exceptions;
using Internship_system.BLL.Extensions;
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
        var internshipProgresses = await _dbContext.InternshipProgresses
            .Include(progress => progress.Student)
            .Include(progress => progress.Company)
            .Include(progress => progress.Comments)
            .ThenInclude(comment => comment.User)
            .Where(progress => progress.Student == student)
            .ToListAsync();
        return internshipProgresses.Select(ip => ip.ToFullDto()).ToList();
    }

    public async Task<List<InternshipDto>> GetStudentInternships(Guid studentId) {
        var student = await _dbContext.Students.FindAsync(studentId) ??
                      throw new NotFoundException($"User with id {studentId} not found");

        var internships = await _dbContext.Internships
            .Include(internship => internship.Student)
            .Include(internship => internship.Company)
            .Include(internship => internship.PracticeDiaries)
            .ThenInclude(practiceDiary => practiceDiary.Comments)
            .ThenInclude(comment => comment.User)
            .Where(internship => internship.Student == student)
            .ToListAsync();
        return internships.Select(i => i.ToFullDto()).ToList();
    }

    public async Task<List<Company>> GetStudentCompanies(Guid studentId) {
        return await _dbContext.InternshipProgresses
            .Include(progress => progress.Company)
            .Where(progress => progress.Student.Id == studentId)
            .Select(progress => progress.Company)
            .ToListAsync();
    }

    public async Task<Guid> AddDesiredCompanyToProgress(InternshipCompanyDto dto) {
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

    public async Task DeleteDesiredCompanyFromProgress(Guid studentId, Guid companyId) {
        await _dbContext.InternshipProgresses.RemoveProgress(studentId, companyId);
    }

    public async Task<IEnumerable<InternshipProgressDto>> UpdateDesiredCompaniesProgress(Guid studentId, List<UpdateInternshipProgressDto> companiesUpdates) {
        var companiesIds = companiesUpdates.Select(cu => cu.CompanyId).ToHashSet();

        var currentCompaniesIds = (await _dbContext.InternshipProgresses
                .Include(progress => progress.Company)
                .Where(progress => progress.Student.Id == studentId)
                .Select(progress => progress.Company.Id)
                .ToListAsync())
            .ToHashSet();

        var companiesToAdd = companiesUpdates.ExceptBy(currentCompaniesIds, dto => dto.CompanyId);
        var companiesToRemove = currentCompaniesIds.Except(companiesIds);
        var companiesToChange = companiesUpdates.IntersectBy(currentCompaniesIds, dto => dto.CompanyId).ToHashSet();

        var newProgresses = new List<InternshipProgress>();

        var student = await _dbContext.Students.FindAsync(studentId) ??
                      throw new NotFoundException($"User with id {studentId} not found");

        foreach (var companyId in companiesToRemove) {
            await _dbContext.InternshipProgresses.RemoveProgress(studentId, companyId);
        }

        foreach (var dto in companiesToAdd) {
            var company = await _dbContext.Companies.FindAsync(dto.CompanyId) ??
                          throw new NotFoundException($"Company with id {dto.CompanyId} not found");
            var progress = _dbContext.InternshipProgresses.CreateAndAttachDefaultProgress(student, company, dto);
            newProgresses.Add(progress);
        }

        foreach (var dto in companiesToChange) {
            var progress = await _dbContext.InternshipProgresses.GetProgressWithAllInclusionsOrThrow(studentId, dto.CompanyId);
            progress.Priority = dto.Priority ?? progress.Priority;
            progress.ProgressStatus = dto.Status ?? ProgressStatus.Default;
            progress.AdditionalInfo = dto.AdditionalInfo;
            _dbContext.InternshipProgresses.Update(progress);
            newProgresses.Add(progress);
        }

        await _dbContext.SaveChangesAsync();

        return newProgresses.Select(ip => ip.ToFullDto());
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
        var internshipProgress = await _dbContext.InternshipProgresses.GetProgressOrThrow(dto.StudentId, dto.CompanyId);

        internshipProgress.ProgressStatus = dto.NewStatus;

        if (dto.NewStatus == ProgressStatus.AcceptedOffer) {
            var student = await _dbContext.Students.FindAsync(dto.StudentId) ??
                          throw new NotFoundException($"User with id {dto.StudentId} not found");
            var company = await _dbContext.Companies.FindAsync(dto.CompanyId) ??
                          throw new NotFoundException($"Company with id {dto.CompanyId} not found");
            var currentInternships = await _dbContext.Internships
                .Where(i=>i.Student == student)
                .Where(i => !i.EndedAt.HasValue)
                .ToListAsync();
            if (currentInternships.Count > 0) {
                foreach (var currentInternship in currentInternships) {
                    currentInternship.EndedAt = DateTime.UtcNow;
                }
                _dbContext.UpdateRange(currentInternships);
            }

            _dbContext.Internships.Add(new() {
                Student = student,
                Company = company,
                PracticeDiaries = [],
                EndedAt = null
            });
        }

        _dbContext.InternshipProgresses.Update(internshipProgress);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<StudentCommentDto> StudentLeaveProgressComment(StudentLeaveProgressCommentDto dto) {
        var internshipProgress = await _dbContext.InternshipProgresses.GetProgressOrThrow(dto.StudentId, dto.CompanyId);
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

    public async Task<InternshipProgress> UpdateInternshipProgress(Guid studentId, UpdateInternshipProgressDto dto) {
        var internshipProgress = await _dbContext.InternshipProgresses.GetProgressOrThrow(studentId, dto.CompanyId);

        if (dto.Priority == null && dto.AdditionalInfo == null) throw new BadRequestException("There must be at least one change");

        internshipProgress.Priority = dto.Priority ?? internshipProgress.Priority;
        internshipProgress.ProgressStatus = dto.Status ?? internshipProgress.ProgressStatus;
        internshipProgress.AdditionalInfo = dto.AdditionalInfo ?? internshipProgress.AdditionalInfo;
        internshipProgress.EditedAt = DateTime.UtcNow;

        _dbContext.Update(internshipProgress);

        return internshipProgress;
    }
}