using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.Core.Entities;

public class Roles
{
    [Key]
    public int Id { get; set; } 
    [Required(ErrorMessage = "Role name is required")]
    public string Name { get; set; }
}