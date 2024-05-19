using System.ComponentModel.DataAnnotations;

namespace Internship_system.BLL.DTOs.PracticeDiary;

public class EditGeneralInfoDto {
    //public string? StudentFullName { get; set; } // ФИО если поле пустое то берем из бд
    //public string? CompanyName { get; set; } // Название компании если поле пустое то берем из бд
    public string? OrderNumber { get; set; } 
    public DateTime? OrderDate { get; set; } 
    public string? CuratorFullName { get; set; } // ФИО руководителя
    public string? StudentCharacteristics { get; set; } // Характеристика
}