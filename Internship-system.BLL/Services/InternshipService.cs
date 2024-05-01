using Internship_system.BLL.DTOs.Internship;
using Internship_system.BLL.DTOs.Internship.Requests;
using Internship_system.BLL.DTOs.Internship.Responses;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;

namespace Internship_system.BLL.Services;

public class InternshipService {
    private readonly InterDbContext _dbContext;

    public InternshipService(InterDbContext dbContext) {
        _dbContext = dbContext;
    }

    // Где-то выше (или здесь) должна быть проверка на то, что студент - именно студент второго курса 
    public async Task<InternshipProgress> AddDesiredCompanyToInternship(InternshipCompanyDto dto) {
        var student = await _dbContext.Students.FindAsync([dto.StudentId]) ??
                      throw new NotFoundException($"User with id {dto.StudentId} not found");
        var company = await _dbContext.Companies.FindAsync([dto.CompanyId]) ??
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

        return new CompanyResponseDto(company.Id, company.Name, company.IsPartner ?? false);
    }


}