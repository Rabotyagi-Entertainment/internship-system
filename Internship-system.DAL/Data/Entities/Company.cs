namespace Internship_system.DAL.Data.Entities;

public class Company {
    public Guid Id { get; set;} = Guid.NewGuid();
    public string Name { get; set; }
    public bool? IsPartner { get; set; }
}