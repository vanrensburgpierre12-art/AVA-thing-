using Microsoft.AspNetCore.Mvc;
using SimDevicePlatform.Core.Interfaces;

namespace SimDevicePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReconciliationController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<ReconciliationController> _logger;

    public ReconciliationController(IDataProcessingService dataProcessingService, ILogger<ReconciliationController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Run reconciliation process
    /// </summary>
    [HttpPost("run")]
    public async Task<ActionResult> RunReconciliation([FromQuery] bool forceRefresh = false)
    {
        try
        {
            _logger.LogInformation("Starting reconciliation process. Force refresh: {ForceRefresh}", forceRefresh);
            
            var result = await _dataProcessingService.RunReconciliationAsync(forceRefresh);
            
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    message = "Reconciliation completed successfully",
                    result = result
                });
            }
            else
            {
                return BadRequest(new
                {
                    message = "Reconciliation failed",
                    error = result.Error,
                    result = result
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconciliation process failed");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> GetSystemHealth()
    {
        try
        {
            var health = await _dataProcessingService.GetSystemHealthAsync();
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system health");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}