using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Internship_system.BLL.Services;

public class InternshipAdminService
{
    private readonly InterDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public InternshipAdminService(InterDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task CreateInternshipCompany(CreateCompanyDto createCompanyDto)
    {
        var company = new Company
        {
            Name = createCompanyDto.Name,
            IsPartner = true
        };

        await _dbContext.Companies.AddAsync(company);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UploadStudents(IFormFile studentsTable)
    {
        if (studentsTable.Length > 0)
        {
            var uploadDir = $"{Directory.GetCurrentDirectory()}/Uploads";
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var filePath = Path.Combine(uploadDir, studentsTable.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await studentsTable.CopyToAsync(stream);
            }
            
            var studentDtos = new List<UploadStudentDto>();
            if (File.Exists(filePath))
            {
                //todo: error here after exporting students as table in current realization 
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage package = new ExcelPackage(new FileInfo(filePath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                for (int i = 0; i < rowCount - 1; i++)
                {
                    var newStudentDto = new UploadStudentDto();
                    studentDtos.Add(newStudentDto);
                }
                
                for (int row = 2; row <= rowCount; row++)
                {
                    studentDtos[row - 2].Fullname = worksheet.Cells[row, 1].Value.ToString();
                    studentDtos[row - 2].Group = worksheet.Cells[row, 2].Value.ToString();
                    studentDtos[row - 2].CourseNumber = int.Parse(worksheet.Cells[row, 3].Value.ToString());
                    studentDtos[row - 2].Email = worksheet.Cells[row, 4].Value.ToString();
                }
                
                package.Dispose();
            }
            
            foreach (var s in studentDtos)
            {
                var findStudent = await _userManager.FindByEmailAsync(s.Email) as Student;
                if (findStudent == null)
                {
                    var student = new Student()
                    {
                        FullName = s.Fullname,
                        UserName = s.Email,
                        Email = s.Email,
                        JoinedAt = DateTime.UtcNow,
                        Group = s.Group,
                        CourseNumber = s.CourseNumber
                    };

                    var result = await _userManager.CreateAsync(student, "qwerty123");
                    if (!result.Succeeded) throw new InvalidOperationException($"Unable to create student user");

                    var studentEntity = await _userManager.FindByIdAsync(student.Id.ToString());
                    await _userManager.AddToRoleAsync(studentEntity, ApplicationRoleNames.Student);
                }
                else
                {
                    findStudent.FullName = s.Fullname;
                    findStudent.Group = s.Group;
                    findStudent.CourseNumber = s.CourseNumber;
                    await _userManager.UpdateAsync(findStudent);
                }
            }
            
            File.Delete(filePath);
            Directory.Delete(uploadDir);
        }
    }

    public async Task ExportStudentsAsTable()
    {
        var uploadDir = $"{Directory.GetCurrentDirectory()}/Uploads";
        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        var filePath = Path.Combine(uploadDir, "exportStudents.xlsx");
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new FileInfo(filePath));
        var workbook = package.Workbook.Worksheets.Add("students");
        workbook.Cells[2, 2].Value = "test";
        
        await package.SaveAsync();

        package.Dispose();
    }

    public async Task<List<StudentStatusDto>> GetStudentCompanies(Guid userId)
    {
        var student = await _dbContext
                       .Students
                       .Include(s => s.InternshipProgresses)
                       .ThenInclude(ip => ip.Company)
                       .Include(s => s.InternshipProgresses)
                       .ThenInclude(ip => ip.Comments)
                       .ThenInclude(c => c.User)
                       .FirstOrDefaultAsync(s => s.Id == userId) ??
                   throw new NotFoundException($"Student with id {userId} not found");

        var companies = new List<StudentStatusDto>();
        foreach (var ip in student.InternshipProgresses)
        {
            var company = new StudentStatusDto
            {
                StudentId = userId,
                CompanyName = ip.Company.Name,
                Status = ip.ProgressStatus,
                InternshipProgressId = ip.Id
            };
            
            var comments = new List<CommentDto>();
            foreach (var comment in ip.Comments)
            {
                var commentDto = new CommentDto
                {
                    Text = comment.Text,
                    Author = comment.User.FullName
                };
                comments.Add(commentDto);
            }

            company.Comments = comments;
            companies.Add(company);
        }

        return companies;
    }

    public async Task CommentInternshipProgress(string comment, Guid internshipProgressId, Guid userId)
    {
        var internshipProgress = await _dbContext
                                     .InternshipProgresses
                                     .Include(ip => ip.Comments)
                                     .FirstOrDefaultAsync(ip => ip.Id == internshipProgressId)
                                 ?? throw new NotFoundException($"Internship progress with id {internshipProgressId} not found");
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new NotFoundException($"User with id {userId} not found");
        var userRole = await _userManager.GetRolesAsync(user);
        
        var newComment = new Comment
        {
            InternshipProgress = internshipProgress,
            RoleType = (RoleType)Enum.Parse(typeof(RoleType), userRole[0]),
            Text = comment,
            User = user
        };
        
        await _dbContext.Comments.AddAsync(newComment);
        internshipProgress.Comments.Add(newComment);
        await _dbContext.SaveChangesAsync();
    } 
}