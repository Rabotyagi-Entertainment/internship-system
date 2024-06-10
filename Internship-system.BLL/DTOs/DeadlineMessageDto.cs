using System.ComponentModel.DataAnnotations;

namespace Internship_system.BLL.DTOs;

public class DeadlineMessageDto {
    [Required]
    [Range(1, 4)]
    public int CourseNumber { get; set; }
    public string? OptionalMessage { get; set; }
    [Required]
    public DateTime DeadlineTime { get; set; }
}