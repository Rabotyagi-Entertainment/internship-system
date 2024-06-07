using Internship_system.BLL.DTOs.InternshipAdmin;
using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.PracticeDiary;

public class PracticeDiaryDto {
    public Guid Id { get; set; } = Guid.NewGuid();
    public PracticeDiaryType DiaryType { get; set; }
    public DiaryState DiaryState { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<CommentDto> Comments { get; set; } = new();

    /// <summary>
    /// Дальше идет инфа для заполнения дневника(общая для всех дневников)
    /// </summary>
    public string? StudentFullName { get; set; } // ФИО
    public string? CuratorFullName { get; set; } // ФИО руководителя
    public string? TaskReportTable { get; set; } // Таблица с задачи
    public string? StudentCharacteristics { get; set; } // Характеристика
    public string? CompanyName { get; set; } // Название компании
    public string? OrderNumber { get; set; } // Номер приказа
    /// <summary>
    /// Поля для курсового дневника
    /// </summary>
    public string? WorkName { get; set; } // Название курсача
    public string? PlanTable { get; set; } // План курсача
}