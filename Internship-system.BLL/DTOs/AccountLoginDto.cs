using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Internship_system.BLL.DTOs;

public class AccountLoginDto {
    /// <summary>
    /// User email
    /// </summary>
    [Required]
    [EmailAddress]
    [DisplayName("email")]
    public required string Email { get; set; }

    /// <summary>
    /// User password
    /// </summary>
    [Required]
    [DefaultValue("qwerty123")]
    [DisplayName("password")]
    public required string Password { get; set; }
}