using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Internship_system.BLL.DTOs;

public class AccountRegisterDto {
    /// <summary>
    /// User`s email
    /// </summary>
    [Required]
    [EmailAddress]
    [DisplayName("email")]
    public required string Email { get; set; }
    
    /// <summary>
    /// User`s tg
    /// </summary>
    [Required]
    [DisplayName("tgName")]
    public required string TelegramUserName { get; set; }

    /// <summary>
    /// User`s password
    /// </summary>
    [Required]
    [DefaultValue("qwerty123")]
    [DisplayName("password")]
    [MinLength(5)]
    public required string Password { get; set; }

    /// <summary>
    /// User`s full name (surname, name, patronymic)
    /// </summary>
    [Required]
    public required string FullName { get; set; }
    
}