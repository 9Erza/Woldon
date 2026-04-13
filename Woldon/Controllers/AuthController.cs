using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Woldon.Data;
using Woldon.Models;

namespace Woldon.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // ==========================================
    // CONFIGURATION & CONSTRUCTOR
    // ==========================================
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // PUBLIC ENDPOINTS
    // ==========================================

    /// <summary>
    /// Authenticates a user and creates a session cookie.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Fetch user from the database
        var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

        // 2. Validate credentials
        // NOTE: Password hashing should be implemented here for production security
        if (user == null || user.PasswordHash != request.Password)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // 3. Prepare identity claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, "Admin") // Defaulting to Admin role for dashboard access
        };

        var claimsIdentity = new ClaimsIdentity(claims, "WoldonAuth");

        // 4. Sign in the user using the configured Authentication Scheme
        await HttpContext.SignInAsync("WoldonAuth", new ClaimsPrincipal(claimsIdentity));

        return Ok(new { success = true });
    }

    /// <summary>
    /// Destroys the current session and clears the authentication cookie.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("WoldonAuth");
        return Ok(new { message = "Successfully logged out" });
    }
}

// ==========================================
// DATA TRANSFER OBJECTS (DTOs)
// ==========================================

/// <summary>
/// Model representing the login data received from the client.
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}