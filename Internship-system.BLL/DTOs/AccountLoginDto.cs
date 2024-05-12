using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Internship_system.BLL.DTOs;

public class AccountLoginDto {
    /// <summary>
    /// User`s tg
    /// </summary>
    [Required]
    [DisplayName("tgName")]
    public required string TelegramUserName { get; set; }

    /// <summary>
    /// User password
    /// </summary>
    [Required]
    [DefaultValue("qwerty123")]
    [DisplayName("password")]
    public required string Password { get; set; }
}