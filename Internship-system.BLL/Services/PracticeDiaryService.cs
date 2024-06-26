using System.IO.Compression;
using Internship_system.BLL.DTOs.InternshipAdmin;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Telegram.Bot;
using Telegram.Bot.Types;
using Xceed.Document.NET;
using Xceed.Words.NET;
using File = System.IO.File;
using InputFile = Microsoft.AspNetCore.Components.Forms.InputFile;

namespace Internship_system.BLL.Services;

public class PracticeDiaryService {
    private readonly InterDbContext _interDbContext;
    private readonly ILogger<PracticeDiaryService> _logger;
    private readonly ITelegramBotClient _telegramBot;

    public PracticeDiaryService(InterDbContext interDbContext, ILogger<PracticeDiaryService> logger, ITelegramBotClient telegramBot) {
        _interDbContext = interDbContext;
        _logger = logger;
        _telegramBot = telegramBot;
    }

    public async Task<List<PracticeDiaryDto>> GetDiaries(Guid? userId, Guid? internshipId) {
        var student = await _interDbContext.Students
            .FirstOrDefaultAsync(s => s.Id == userId);
        if (student == null && userId != null)
            throw new NotFoundException("Student not found");
        var internship = await _interDbContext.Internships
            .Include(internship => internship.Student)
            .FirstOrDefaultAsync(s => s.Id == internshipId);
        if (internship == null && internshipId != null)
            throw new NotFoundException("Internship not found");
        var diaries = await _interDbContext.PracticeDiaries
            .Where(pd =>
                (student == null ||
                 pd.Internship.Student == student)
                && (internship == null ||
                    pd.Internship == internship))
            .Include(practiceDiary => practiceDiary.Internship)
            .ThenInclude(ip => ip.Company)
            .Include(practiceDiary => practiceDiary.Comments)
            .ThenInclude(comment => comment.User)
            .ToListAsync();

        return diaries.Select(d => new PracticeDiaryDto {
            Id = d.Id,
            DiaryType = d.DiaryType,
            DiaryState = d.DiaryState,
            CreatedAt = d.CreatedAt,
            StudentFullName = internship?.Student.FullName,
            CuratorFullName = d.CuratorFullName,
            TaskReportTable = d.TaskReportTable,
            StudentCharacteristics = d.StudentCharacteristics,
            CompanyName = d.Internship.Company.Name,
            OrderNumber = d.OrderNumber,
            WorkName = d.WorkName,
            PlanTable = d.PlanTable,
            Comments = d.Comments
                .OrderByDescending(c=>c.CreatedAt)
                .Select(c => new CommentDto {
                    Text = c.Text,
                    Author = c.User.FullName,
                    RoleType = c.RoleType
                }).ToList()
        }).ToList();
    }

    public async Task<MemoryStream> GetZipDiariesByCourseNumber(int courseNumber) {
        var students = await _interDbContext.Students
            .Where(s => s.CourseNumber ==courseNumber)
            .ToListAsync();
        var diaries = await _interDbContext.PracticeDiaries
            .Where(pd => (pd.DiaryState == DiaryState.Done || pd.DiaryState == DiaryState.DeanApproved || pd.DiaryState == DiaryState.OnDeanCheck)
                && students.Contains(pd.Internship.Student))
            .ToListAsync();
        var zipStream = new MemoryStream();
        using ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
        foreach (var diary in diaries) {
            var (stream, name) = await GetDiaryFile(diary.Id);
            stream.Position = 0; 
            AddMemoryStreamToZip(archive, stream, $"Дневник практики {name}.docx");
        }

        return zipStream;
    }
    private void AddMemoryStreamToZip(ZipArchive archive, MemoryStream stream, string entryName)
    {
        var entry = archive.CreateEntry(entryName);
        using (var entryStream = entry.Open())
        {
            stream.CopyTo(entryStream);
        }
    }
    public async Task GetDiaryFileTg(Guid diaryId, Guid userId) {
        var (stream, _) = await GetDiaryFile(diaryId);
        var student = await _interDbContext.Students.FindAsync(userId) ??
                      throw new NotFoundException($"User with id {userId} not found");
        var tgUser = await _interDbContext.StudentTelegrams
            .FirstOrDefaultAsync(st => st.TgName == student.UserName);
        if (tgUser == null)
            throw new NotFoundException($"Tg with username id {student.UserName} not found");
        stream.Position = 0; 
        var inputOnlineFile = new InputFileStream(stream, $"Дневник практики {student.FullName}.docx");
        await _telegramBot.SendDocumentAsync(tgUser.ChatId, inputOnlineFile);
    }
    public async Task<(MemoryStream stream, string fileName)> GetDiaryFile(Guid diaryId) {
        var diary = await _interDbContext.PracticeDiaries
            .Include(d => d.Internship)
            .ThenInclude(i => i.Company)
            .Include(practiceDiary => practiceDiary.Internship)
            .ThenInclude(internship => internship.Student)
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        var filePath = diary.DiaryType == PracticeDiaryType.Default
            ? "Src/PracticeDiary_Default.docx"
            : "Src/PracticeDiary_CourseWork.docx";
        _logger.LogInformation($"////////////{filePath} ///////////\n");
        _logger.LogInformation($"////////////{Directory.GetCurrentDirectory()} ///////////\n");
        var directories = Directory.GetDirectories(Directory.GetCurrentDirectory());
        foreach (var dir in directories)
        {
            _logger.LogInformation($"sub dirs: {dir}");
        }
        var byteArray = File.OpenRead(filePath);
        var sourceDoc = DocX.Load(byteArray);
        foreach (var paragraph in sourceDoc.Paragraphs) {
            if (paragraph.Text.Contains("ФИО обучающегося")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.Internship.Student.FullName,
                    SearchValue = "ФИО обучающегося",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("3 курс")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.Internship.Student.CourseNumber == null
                        ? diary.DiaryType == PracticeDiaryType.Default
                            ? "2 курс"
                            : diary.DiaryType == PracticeDiaryType.CourseWork
                                ? "3 курс"
                                : "4 курс"
                        : diary.Internship.Student.CourseNumber + " курс",
                    SearchValue = "3 курс",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("Полное название профильной организации")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.Internship.Company.Name,
                    SearchValue = "Полное название профильной организации",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("Номер_приказа")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.OrderNumber ?? "НЕ ЗАПОЛНЕНО",
                    SearchValue = "Номер_приказа",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("Дата_приказа")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = !diary.OrderDate.HasValue
                        ? "НЕ ЗАПОЛНЕНО"
                        : diary.OrderDate!.Value.ToShortDateString(),
                    SearchValue = "Дата_приказа",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("ФИО руководителя от профильной организации (куратора практики)")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.CuratorFullName ?? "НЕ ЗАПОЛНЕНО",
                    SearchValue = "ФИО руководителя от профильной организации (куратора практики)",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (paragraph.Text.Contains("Характеристика")) {
                var replace = new StringReplaceTextOptions {
                    NewValue = diary.StudentCharacteristics ?? "НЕ ЗАПОЛНЕНО",
                    SearchValue = "Характеристика",
                    NewFormatting = new Formatting {
                        Bold = false,
                    }
                };
                paragraph.ReplaceText(replace);
            }

            if (diary.DiaryType is PracticeDiaryType.CourseWork or PracticeDiaryType.GraduationWork) {
                if (paragraph.Text.Contains("Название_курсовой")) {
                    var replace = new StringReplaceTextOptions {
                        NewValue = diary.WorkName ?? "НЕ ЗАПОЛНЕНО",
                        SearchValue = "Название_курсовой",
                        NewFormatting = new Formatting {
                            Bold = false,
                        }
                    };
                    paragraph.ReplaceText(replace);
                }

                if (paragraph.Text.Contains("Обзор_методов")) {
                    var replace = new StringReplaceTextOptions {
                        NewValue = diary.PlanTable ?? "НЕ ЗАПОЛНЕНО",
                        SearchValue = "Обзор_методов",
                        NewFormatting = new Formatting {
                            Bold = false,
                        }
                    };
                    paragraph.ReplaceText(replace);
                }
            }
        }

        var ph = sourceDoc.Paragraphs
            .FirstOrDefault(p => p.Text.Contains("Отчет о выполненных задачах"));
        if (ph != null && diary.TaskReportTable != null) {
            var rows = diary.TaskReportTable.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var table = sourceDoc.AddTable(rows.Length + 1, 3);
            table.Design = TableDesign.TableGrid;
            table.Rows[0].Cells[0].Paragraphs[0].Append("Дата начала");
            table.Rows[0].Cells[1].Paragraphs[0].Append("Дата окончания");
            table.Rows[0].Cells[2].Paragraphs[0].Append("Название задачи");


            for (var i = 0; i < rows.Length; i++) {
                var rowData = rows[i].Trim().Split(';');
                for (var j = 0; j < rowData.Length; j++) {
                    table.Rows[i + 1].Cells[j].Paragraphs[0].Append(rowData[j]);
                }
            }

            ph.InsertTableAfterSelf(table);
        }

        MemoryStream ms = new MemoryStream();

        sourceDoc.SaveAs(ms);
        ms.Position = 0;

        return (ms, diary.Internship.Student.FullName);
    }

    public async Task CreateDiary(Guid internshipId, PracticeDiaryType diaryType) {
        var internship = await _interDbContext.Internships
            .FirstOrDefaultAsync(s => s.Id == internshipId);
        if (internship == null)
            throw new NotFoundException("Internship not found");
        var diary = new PracticeDiary {
            DiaryType = diaryType,
            Internship = internship
        };
        _interDbContext.Add(diary);
        await _interDbContext.SaveChangesAsync();
    }
    
    public async Task DeleteDiary(Guid diaryId) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        _interDbContext.Remove(diary);
        await _interDbContext.SaveChangesAsync();
    }
    
    public async Task EditGeneralInfo(Guid diaryId, EditGeneralInfoDto dto) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        diary.CuratorFullName = dto.CuratorFullName;
        diary.StudentCharacteristics = dto.StudentCharacteristics;
        diary.OrderDate = dto.OrderDate;
        diary.OrderNumber = dto.OrderNumber;
        _interDbContext.Update(diary);
        await _interDbContext.SaveChangesAsync();
    }

    public async Task EditAdditionalInfo(Guid diaryId, EditAdditionalInfoDto dto) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        diary.WorkName = dto.WorkName;
        diary.PlanTable = dto.PlanTable;
        _interDbContext.Update(diary);
        await _interDbContext.SaveChangesAsync();
    }

    public async Task LoadXlsFile(Guid diaryId, IFormFile file) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var result = "";
        for (var row = 2; row <= worksheet.Dimension.Rows; row++) {
            var startDate = worksheet.Cells[row, 1].Text;
            var endDate = worksheet.Cells[row, 2].Text;
            var taskName = worksheet.Cells[row, 3].Text;

            result += $"{startDate};{endDate};{taskName}\n";
        }

        diary.TaskReportTable = result;
        _interDbContext.Update(diary);
        await _interDbContext.SaveChangesAsync();
    }

    public async Task<CommentDto> LeavePracticeDiaryComment(LeavePracticeDiaryCommentDto dto) {
        var practiceDiary = await _interDbContext.PracticeDiaries.FirstOrDefaultAsync(pd => pd.Id == dto.DiaryId) ??
                            throw new NotFoundException($"Can't find practice diary with id {dto.DiaryId}");
        var user = await _interDbContext.Users.FindAsync(dto.UserId) ??
                   throw new NotFoundException($"User with id {dto.UserId} not found");

        var comment = new Comment {
            PracticeDiary = practiceDiary,
            InternshipProgress = null,
            User = user,
            RoleType = RoleType.Student,
            Text = dto.Text
        };
        _interDbContext.Comments.Add(comment);

        await _interDbContext.SaveChangesAsync();

        return new() {
            Text = comment.Text,
            Author = user.FullName,
            RoleType = comment.RoleType
        };
    }
}