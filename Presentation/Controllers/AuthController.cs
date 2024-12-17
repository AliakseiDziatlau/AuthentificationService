using AuthentificationService.Application.DTOs;
using AuthentificationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        await _authService.RegisterUserAsync(dto);
        return Ok(new {Message = "Registration successful! Check your email for varification"});
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        await _authService.ConfirmEmailAsync(token, email);
        return Ok(new { Message = "Email confirmed successfully! You can now log in!" });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var token = await _authService.LoginUserAsync(dto);
        return Ok(new {Token = token});
    }
}