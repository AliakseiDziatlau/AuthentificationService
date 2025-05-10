namespace AuthentificationService.Application.DTOs;

public class AccountsDTO
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public bool IsEmailVerified { get; set; }
}
