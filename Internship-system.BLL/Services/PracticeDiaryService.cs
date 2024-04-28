using System.Drawing;
using System.Text.RegularExpressions;
using Internship_system.BLL.DTOs.PracticeDiary;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Internship_system.BLL.Services;

public class PracticeDiaryService {

    private readonly InterDbContext _interDbContext;

    public PracticeDiaryService(InterDbContext interDbContext) {
        _interDbContext = interDbContext;
    }

    public async Task<List<PracticeDiaryDto>> GetDiaries(Guid userId) {
        var student = await _interDbContext.Students
            .FirstOrDefaultAsync(s => s.Id == userId);
        if (student == null)
            throw new NotFoundException("Student not found");
        var diaries = await _interDbContext.PracticeDiaries
            .Where(pd => pd.Internship.Student == student).Include(practiceDiary => practiceDiary.Internship)
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
            OrderNumber = d.Internship.OrderNumber,
            WorkName = d.WorkName,
            PlanTable = d.PlanTable
        }).ToList();

    }
      
    public async Task<MemoryStream> GetDiaryFile(Guid diaryId) {
        var diary = await _interDbContext.PracticeDiaries
            .Include(d => d.Internship)
            .ThenInclude(i => i.Company)
            .Include(practiceDiary => practiceDiary.Internship)
            .ThenInclude(internship => internship.Student)
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        string filePath = "../internship-system.Common/Src/PracticeDiary_CourseWork.docx";

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
                        NewValue = diary.Internship.OrderNumber,
                        SearchValue = "Номер_приказа",
                        NewFormatting = new Formatting {
                            Bold = false,
                        }
                    };
                    paragraph.ReplaceText(replace);
                } 
                if (paragraph.Text.Contains("ФИО руководителя от профильной организации (куратора практики)")) {
                    var replace = new StringReplaceTextOptions {
                        NewValue = diary.CuratorFullName,
                        SearchValue = "ФИО руководителя от профильной организации (куратора практики)",
                        NewFormatting = new Formatting {
                            Bold = false,
                        }
                    };
                    paragraph.ReplaceText(replace);
                } 
                if (paragraph.Text.Contains("Характеристика")) {
                    var replace = new StringReplaceTextOptions {
                        NewValue = diary.StudentCharacteristics, 
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
                            NewValue = diary.WorkName,
                            SearchValue = "Название_курсовой",
                            NewFormatting = new Formatting {
                                Bold = false,
                            }
                        };
                        paragraph.ReplaceText(replace);
                    } 
                    if (paragraph.Text.Contains("Обзор_методов")) {
                        var replace = new StringReplaceTextOptions {
                            NewValue = diary.PlanTable,
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

            return ms;
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
    
    public async Task EditGeneralInfo(Guid diaryId, EditGeneralInfoDto dto) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        diary.CuratorFullName = dto.CuratorFullName;
        diary.StudentCharacteristics = dto.StudentCharacteristics;
        _interDbContext.Update(diary);
        await _interDbContext.SaveChangesAsync();
    } 

    public async Task EditAdditionalInfo(Guid diaryId, EditAdditionalInfoDto dto) {
        var diary = await _interDbContext.PracticeDiaries
            .FirstOrDefaultAsync(s => s.Id == diaryId);
        if (diary == null)
            throw new NotFoundException("Diary not found");
        diary.WorkName = dto.WorkName;
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
        for (var row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            var startDate = worksheet.Cells[row, 1].Text; 
            var endDate = worksheet.Cells[row, 2].Text; 
            var taskName = worksheet.Cells[row, 3].Text;

            result += $"{startDate};{endDate};{taskName}\n";
        }
        diary.TaskReportTable = result;
        _interDbContext.Update(diary);
        await _interDbContext.SaveChangesAsync();
    }
}