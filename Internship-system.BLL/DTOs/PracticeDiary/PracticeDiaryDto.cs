using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.PracticeDiary;

public class PracticeDiaryDto {
    public Guid Id { get; set; } = Guid.NewGuid();
    public PracticeDiaryType DiaryType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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