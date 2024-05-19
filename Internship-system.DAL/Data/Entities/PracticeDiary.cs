using internship_system.Common.Enums;

namespace Internship_system.DAL.Data.Entities;

public class PracticeDiary {
    public Guid Id { get; set; } = Guid.NewGuid();
    public PracticeDiaryType DiaryType { get; set; }
    public Internship Internship { get; set; }
    public byte[]? File { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime EditedAt { get; set; }
    public string? OrderNumber { get; set; } // номер приказа
    public DateTime? OrderDate { get; set; }
    
    /// <summary>
    /// Дальше идет инфа для заполнения дневника(общая для всех дневников)
    /// </summary>
    public string? CuratorFullName { get; set; } // ФИО руководителя
    public string? TaskReportTable { get; set; } // Таблица с задачи
    public string? StudentCharacteristics { get; set; } // Характеристика
    // Название компании из Company
    // Номер приказа из Internship
    /// <summary>
    /// Поля для курсового/дипломного дневника
    /// </summary>
    public string? WorkName { get; set; } // Название курсача/диплома
    public string? PlanTable { get; set; } // План курсача/диплома
}