using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "SupportPulse API",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("crash-test")]
public IActionResult TriggerCrash()
{
    throw new Exception("Sentry Crash Test successful!");
}
}