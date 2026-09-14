using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Server-side authorization on every route (bina token ke koi access nahi)
public class ProfileController : ControllerBase
{
    // API accessible to everyone who is logged in (Customer, Agent, Admin)
    [HttpGet("me")]
    public IActionResult GetMyProfile()
    {
        // BOLA/IDOR Protection: 
        // Hum user ki ID URL (e.g. /profile/1) se lene ki bajaye, directly secure JWT token se le rahe hain.
        // Koi bhi user URL manipulate karke kisi aur ka data nahi dekh payega.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new { message = "You are authenticated!", userId, role });
    }

    // API accessible ONLY to Agents and Admins (RBAC applied)
    [HttpGet("agent-only")]
    [Authorize(Roles = "Agent,Admin")] 
    public IActionResult GetAgentData()
    {
        return Ok(new { message = "This is secured agent data. Customers cannot see this!" });
    }
}