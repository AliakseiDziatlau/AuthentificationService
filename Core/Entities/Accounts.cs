using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.Core.Entities;

public class Accounts
{
    [Key]
    public int id { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string passwordHash { get; set; }
    [Required(ErrorMessage = "Phone number is required")]
    public string phoneNumber { get; set; }
    public bool isEmailVerified { get; set; }
    public int documentsId { get; set; }
    public int createdBy { get; set; }
    public DateTime createdAt { get; set; } = DateTime.UtcNow;
    public int? updatedBy { get; set; }
    public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
}