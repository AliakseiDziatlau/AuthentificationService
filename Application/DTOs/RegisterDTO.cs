using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.Application.DTOs;

public class RegisterDTO
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, MinLength(6)]
    public string Password { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string Role { get; set; }
}