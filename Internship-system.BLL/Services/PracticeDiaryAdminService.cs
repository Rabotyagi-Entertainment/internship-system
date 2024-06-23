using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.DTOs.PracticeDiaryAdmin;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Internship_system.BLL.Services;

public class PracticeDiaryAdminService {
    private readonly InterDbContext _dbContext;

    public PracticeDiaryAdminService(InterDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<MemoryStream> ExportStudentsInternshipsAsTable() {
        var uploadDir = $"{Directory.GetCurrentDirectory()}/Uploads";
        if (!Directory.Exists(uploadDir)) {
            Directory.CreateDirectory(uploadDir);
        }

        var filePath = Path.Combine(uploadDir, "exportStudentsInternships.xlsx");

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var package = new ExcelPackage(new FileInfo(filePath));
        var workbook = package.Workbook.Worksheets.Add("studentsInternships");
        workbook.Cells[1, 1].Value = "Fullname";
        workbook.Cells[1, 2].Value = "Group";
        workbook.Cells[1, 3].Value = "Company";
        workbook.Cells[1, 4].Value = "Email";
        workbook.Cells[1, 5].Value = "Telegram";

        var students = await _dbContext
            .Students
            .Include(s => s.Internships)
            .ThenInclude(i => i.Company)
            .ToListAsync();
        for (int i = 0; i < students.Count; i++) {
            workbook.Cells[i + 2, 1].Value = students[i].FullName;
            workbook.Cells[i + 2, 2].Value = students[i].Group;
            workbook.Cells[i + 2, 4].Value = students[i].Email;
            workbook.Cells[i + 2, 5].Value = students[i].UserName;

            var company = students[i].Internships.FirstOrDefault(i => i.EndedAt == null);
            if (company != null) {
                var companyName = company.Company.Name;
                workbook.Cells[i + 2, 3].Value = companyName;
            }
            else {
                workbook.Cells[i + 2, 3].Value = "null";
            }
        }

        var result = new MemoryStream();
        result.Position = 0;
        await package.SaveAsAsync(result);

        return result;
    }

    public async Task<DiaryState> GetStudentDiaryState(Guid diaryId) {
        var practiceDiary = await _dbContext
                                .PracticeDiaries
                                .FirstOrDefaultAsync(pd => pd.Id == diaryId) ??
                            throw new NotFoundException($"Can't find practice diary with id {diaryId}");
        return practiceDiary.DiaryState;
    }

    public async Task ChangeDiaryStatus(DiaryState diaryState, Guid diaryId) {
        var practiceDiary = await _dbContext
                                .PracticeDiaries
                                .FirstOrDefaultAsync(pd => pd.Id == diaryId) ??
                            throw new NotFoundException($"Can't find practice diary with id {diaryId}");
        practiceDiary.DiaryState = diaryState;
    }

    public async Task<CommentDto> LeavePracticeDiaryComment(LeavePracticeDiaryCommentDto dto) {
        var practiceDiary = await _dbContext.PracticeDiaries.FirstOrDefaultAsync(pd => pd.Id == dto.DiaryId) ??
                            throw new NotFoundException($"Can't find practice diary with id {dto.DiaryId}");
        var user = await _dbContext.Users.FindAsync(dto.UserId) ??
                   throw new NotFoundException($"User with id {dto.UserId} not found");

        var comment = new Comment {
            PracticeDiary = practiceDiary,
            InternshipProgress = null,
            User = user,
            RoleType = RoleType.Dean,
            Text = dto.Text
        };
        _dbContext.Comments.Add(comment);

        await _dbContext.SaveChangesAsync();

        return new() {
            Text = comment.Text,
            Author = user.FullName,
            RoleType = comment.RoleType
        };
    }

    public async Task<List<StudentListElemDto>> GetStudentsList(string fullName) {
        var students = await _dbContext
            .Students
            .Where(s => s.FullName.Contains(fullName))
            .ToListAsync();

        var studentList = new List<StudentListElemDto>();
        foreach (var s in students) {
            var studentDto = new StudentListElemDto {
                StudentId = s.Id,
                FullName = s.FullName
            };
            studentList.Add(studentDto);
        }

        return studentList;
    }

    public async Task<List<PracticeDiaryDto>> GetPracticeDiariesByInternshipId(Guid internshipId) {
        var student = await _dbContext.Internships
                          .Where(internship => internship.Id == internshipId)
                          .Include(internship => internship.Student)
                          .Select(internship => internship.Student)
                          .FirstOrDefaultAsync()
                      ?? throw new NotFoundException("Internship not found");

        var diaries = await _dbContext.PracticeDiaries
            .Where(pd => pd.Internship.Student == student)
            .Include(practiceDiary => practiceDiary.Internship)
            .ThenInclude(internship => internship.Company)
            .ToListAsync();

        return diaries.Select(d => new PracticeDiaryDto {
            Id = d.Id,
            DiaryType = d.DiaryType,
            StudentFullName = student.FullName,
            CuratorFullName = d.CuratorFullName,
            TaskReportTable = d.TaskReportTable,
            StudentCharacteristics = d.StudentCharacteristics,
            CompanyName = d.Internship.Company.Name,
            WorkName = d.WorkName,
            PlanTable = d.PlanTable
        }).ToList();
    }
}