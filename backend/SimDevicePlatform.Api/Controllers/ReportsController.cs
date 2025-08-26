using Microsoft.AspNetCore.Mvc;
using SimDevicePlatform.Core.Entities;
using SimDevicePlatform.Core.Interfaces;

namespace SimDevicePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IDataProcessingService dataProcessingService, ILogger<ReportsController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a new report
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        try
        {
            var report = await _dataProcessingService.GenerateReportAsync(request.Type);
            
            return Ok(new
            {
                message = "Report generated successfully",
                report = new
                {
                    report.ReportId,
                    report.Type,
                    report.Status,
                    report.GeneratedAt,
                    report.RowCount,
                    report.FileSizeBytes,
                    downloadUrl = $"/api/reports/{report.ReportId}/download"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportType}", request.Type);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Download a generated report
    /// </summary>
    [HttpGet("{reportId}/download")]
    public async Task<ActionResult> DownloadReport(Guid reportId)
    {
        try
        {
            // In a real implementation, you would retrieve the report from storage
            // and return the file. For now, we'll return a placeholder response.
            
            return Ok(new
            {
                message = "Report download endpoint",
                reportId = reportId,
                note = "File download implementation would go here"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download report {ReportId}", reportId);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get available report types
    /// </summary>
    [HttpGet("types")]
    public ActionResult GetReportTypes()
    {
        var reportTypes = Enum.GetValues<ReportType>()
            .Select(rt => new
            {
                value = rt.ToString(),
                name = GetReportTypeDisplayName(rt),
                description = GetReportTypeDescription(rt)
            });

        return Ok(reportTypes);
    }
}

public class GenerateReportRequest
{
    public ReportType Type { get; set; }
}

public static class ReportTypeExtensions
{
    public static string GetReportTypeDisplayName(this ReportType type)
    {
        return type switch
        {
            ReportType.ActiveLinkedDevices => "Active Linked Devices",
            ReportType.InactiveDevices => "Inactive Devices",
            ReportType.SimButNoAsset => "SIM but No Asset",
            ReportType.AssetButNoSim => "Asset but No SIM",
            ReportType.NoLinkageOrphaned => "No Linkage (Orphaned)",
            ReportType.UnmatchedSims => "Unmatched SIMs",
            _ => type.ToString()
        };
    }

    public static string GetReportTypeDescription(this ReportType type)
    {
        return type switch
        {
            ReportType.ActiveLinkedDevices => "Devices with both SIM and asset linkages",
            ReportType.InactiveDevices => "Devices marked as inactive",
            ReportType.SimButNoAsset => "Devices linked to SIM but missing asset",
            ReportType.AssetButNoSim => "Devices linked to asset but missing SIM",
            ReportType.NoLinkageOrphaned => "Devices without SIM or asset linkages",
            ReportType.UnmatchedSims => "SIMs not linked to any device",
            _ => "Unknown report type"
        };
    }
}