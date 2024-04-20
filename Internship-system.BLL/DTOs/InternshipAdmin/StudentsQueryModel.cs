using Internship_system.DAL.Data.Entities.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public class StudentsQueryModel
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }
    
    [FromQuery(Name = "status")]
    public ProgressStatus? Status { get; set; }
    
    [FromQuery(Name = "company")]
    public string? Company { get; set; }
    
    [FromQuery(Name = "group")]
    public string? Group { get; set; }
}