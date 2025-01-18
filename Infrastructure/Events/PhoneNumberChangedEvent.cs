namespace AuthentificationService.Infrastructure.Events;

public class PhoneNumberChangedEvent
{
    public string UserEmail { get; set; } 
    public string NewPhoneNumber { get; set; }
    public DateTime Timestamp { get; set; } 
    public int RoleId { get; set; }
}