using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.Application.DTOs;

public class UpdateUserDTO
{
    [EmailAddress]
    public string Email { get; set; }
    [Phone]
    public string PhoneNumber { get; set; }
}