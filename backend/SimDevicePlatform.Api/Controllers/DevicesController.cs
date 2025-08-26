using Microsoft.AspNetCore.Mvc;
using SimDevicePlatform.Core.Interfaces;
using SimDevicePlatform.Core.Models;

namespace SimDevicePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IDataProcessingService dataProcessingService, ILogger<DevicesController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Get unified device view with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UnifiedDeviceViewResult>> GetDevices(
        [FromQuery] string? q,
        [FromQuery] string? oem,
        [FromQuery] string? status,
        [FromQuery] string? account,
        [FromQuery] bool? hasAsset,
        [FromQuery] bool? hasSim,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var filter = new UnifiedDeviceViewFilter
            {
                SearchQuery = q,
                Oem = !string.IsNullOrEmpty(oem) ? Enum.Parse<DeviceOem>(oem) : null,
                Status = !string.IsNullOrEmpty(status) ? Enum.Parse<DeviceStatus>(status) : null,
                Account = account,
                HasAsset = hasAsset,
                HasSim = hasSim,
                Page = page,
                PageSize = pageSize
            };

            var result = await _dataProcessingService.GetUnifiedDeviceViewAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get devices");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Link a device to a SIM
    /// </summary>
    [HttpPost("link-sim")]
    public async Task<ActionResult> LinkDeviceToSim([FromBody] LinkDeviceSimRequest request)
    {
        try
        {
            var success = await _dataProcessingService.LinkDeviceToSimAsync(
                request.DeviceId, 
                request.Iccid, 
                request.Source, 
                request.Confidence);

            if (success)
            {
                return Ok(new { message = "Device linked to SIM successfully" });
            }

            return BadRequest(new { message = "Failed to link device to SIM" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link device {DeviceId} to SIM {Iccid}", request.DeviceId, request.Iccid);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Link a device to an asset
    /// </summary>
    [HttpPost("link-asset")]
    public async Task<ActionResult> LinkDeviceToAsset([FromBody] LinkDeviceAssetRequest request)
    {
        try
        {
            var success = await _dataProcessingService.LinkAssetToDeviceAsync(
                request.AssetId, 
                request.DeviceId, 
                request.MatchBasis);

            if (success)
            {
                return Ok(new { message = "Device linked to asset successfully" });
            }

            return BadRequest(new { message = "Failed to link device to asset" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link device {DeviceId} to asset {AssetId}", request.DeviceId, request.AssetId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unlink a device from a SIM
    /// </summary>
    [HttpDelete("unlink-sim")]
    public async Task<ActionResult> UnlinkDeviceFromSim([FromBody] UnlinkDeviceSimRequest request)
    {
        try
        {
            var success = await _dataProcessingService.UnlinkDeviceFromSimAsync(request.DeviceId, request.Iccid);

            if (success)
            {
                return Ok(new { message = "Device unlinked from SIM successfully" });
            }

            return BadRequest(new { message = "Failed to unlink device from SIM" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink device {DeviceId} from SIM {Iccid}", request.DeviceId, request.Iccid);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unlink a device from an asset
    /// </summary>
    [HttpDelete("unlink-asset")]
    public async Task<ActionResult> UnlinkDeviceFromAsset([FromBody] UnlinkDeviceAssetRequest request)
    {
        try
        {
            var success = await _dataProcessingService.UnlinkAssetFromDeviceAsync(request.AssetId, request.DeviceId);

            if (success)
            {
                return Ok(new { message = "Device unlinked from asset successfully" });
            }

            return BadRequest(new { message = "Failed to unlink device from asset" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink device {DeviceId} from asset {AssetId}", request.DeviceId, request.AssetId);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class LinkDeviceSimRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string Iccid { get; set; } = string.Empty;
    public LinkSource Source { get; set; }
    public float Confidence { get; set; }
}

public class LinkDeviceAssetRequest
{
    public string AssetId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public MatchBasis MatchBasis { get; set; }
}

public class UnlinkDeviceSimRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string Iccid { get; set; } = string.Empty;
}

public class UnlinkDeviceAssetRequest
{
    public string AssetId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}