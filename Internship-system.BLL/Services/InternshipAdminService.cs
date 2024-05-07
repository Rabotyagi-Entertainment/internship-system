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
                    if (CellExists(worksheet, i + 2, 1))
                    {
                        var newStudentDto = new UploadStudentDto();
                        studentDtos.Add(newStudentDto);
                    }
                }
                
                for (int row = 2; row <= rowCount; row++)
                {
                    if(CellExists(worksheet, row, 1)) studentDtos[row - 2].Fullname = worksheet.Cells[row, 1].Value.ToString();
                    if(CellExists(worksheet, row, 2)) studentDtos[row - 2].Group = worksheet.Cells[row, 2].Value.ToString();
                    if(CellExists(worksheet, row, 3)) studentDtos[row - 2].CourseNumber = int.Parse(worksheet.Cells[row, 3].Value.ToString());
                    if(CellExists(worksheet, row, 4)) studentDtos[row - 2].Email = worksheet.Cells[row, 4].Value.ToString();
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

            var studentEntities = await _dbContext.Students.ToListAsync();
            foreach (var studentEntity in studentEntities)
            {
                if (studentDtos.FirstOrDefault(std => std.Email == studentEntity.Email) == null)
                {
                    await _userManager.DeleteAsync(studentEntity);
                }
            }
            
            File.Delete(filePath);
            Directory.Delete(uploadDir, true);
        }
    }

    public async Task<MemoryStream> ExportStudentsAsTable()
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
        workbook.Cells[1, 1].Value = "Fullname";
        workbook.Cells[1, 2].Value = "Group";
        workbook.Cells[1, 3].Value = "CourseNumber";
        workbook.Cells[1, 4].Value = "Email";

        var students = await _dbContext
            .Students
            .ToListAsync();
        for (int i = 0; i < students.Count; i++)
        {
            workbook.Cells[i + 2, 1].Value = students[i].FullName;
            workbook.Cells[i + 2, 2].Value = students[i].Group;
            workbook.Cells[i + 2, 3].Value = students[i].CourseNumber;
            workbook.Cells[i + 2, 4].Value = students[i].Email;
        }
        
        var result = new MemoryStream();
        result.Position = 0;
        await package.SaveAsAsync(result);
        
        return result;    }

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

    public async Task<List<StudentInfoDto>> GetStudentsList(StudentsQueryModel query)
    {
        var students = await _dbContext
            .Students
            .Include(s => s.InternshipProgresses)
            .ThenInclude(ip => ip.Company)
            .ToListAsync();

        if (query.Search != null)
        {
            students = students.Where(s => s.FullName.Contains(query.Search)).ToList();
        }

        if (query.Group != null)
        {
            students = students.Where(s => s.Group == query.Group).ToList();
        }

        if (query.Company != null)
        {
            students = students
                .Where(s => s
                    .InternshipProgresses
                    .Any(ip => ip.Company.Name.Contains(query.Company)))
                .ToList();
        }

        List<StudentInfoDto> studentInfoList = new();
        foreach (var s in students)
        {
            var studentDto = new StudentInfoDto
            {
                Id = s.Id,
                Name = s.FullName,
                Group = s.Group
            };
            foreach (var ip in s.InternshipProgresses)
            {
                studentDto.Companies.Add(ip.Company.Name);
            }
            
            studentInfoList.Add(studentDto);
        }

        return studentInfoList;
    }
    

    private bool CellExists(ExcelWorksheet worksheet, int row, int col)
    {
        try
        {
            var cell = worksheet.Cells[row, col].Value.ToString();
            if (cell != null && (string)cell != "") return true;
            return false;
        }
        catch
        {
            return false;
        }
    }
}