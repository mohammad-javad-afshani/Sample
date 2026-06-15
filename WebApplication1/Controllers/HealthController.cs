using Application.Health;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(ISender sender)
    {
        var status = await sender.Send(new GetHealthStatusQuery());

        _logger.LogInformation(
            "Health check requested. Connection string: {ConnectionString}",
            status.DatabaseConnectionString);

        return Ok(status);
    }
}
