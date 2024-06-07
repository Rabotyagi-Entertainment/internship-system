namespace Internship_system.DAL.Data.Entities;

public class StudentTelegram {
    public Guid Id { get; set;} = Guid.NewGuid();
    public string TgName { get; set; }
    public string ChatId { get; set; }
}