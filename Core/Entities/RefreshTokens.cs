using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.Core.Entities;

public class RefreshTokens
{
    [Key]
    public int Id { get; set; } 
    [Required(ErrorMessage = "Refresh token is required")]
    public string Token { get; set; } 
    public DateTime ExpiryDate { get; set; }
    public int AccountId { get; set; }  
}