using SimDevicePlatform.Core.Models;

namespace SimDevicePlatform.Core.Interfaces;

public interface IDataProcessingService
{
    Task<ReconciliationResult> RunReconciliationAsync(bool forceRefresh = false, CancellationToken ct = default);
    Task<UnifiedDeviceViewResult> GetUnifiedDeviceViewAsync(UnifiedDeviceViewFilter filter, CancellationToken ct = default);
    Task<bool> LinkDeviceToSimAsync(string deviceId, string iccid, LinkSource source, float confidence, CancellationToken ct = default);
    Task<bool> LinkAssetToDeviceAsync(string assetId, string deviceId, MatchBasis basis, CancellationToken ct = default);
    Task<bool> UnlinkDeviceFromSimAsync(string deviceId, string iccid, CancellationToken ct = default);
    Task<bool> UnlinkAssetFromDeviceAsync(string assetId, string deviceId, CancellationToken ct = default);
    Task<Report> GenerateReportAsync(ReportType type, CancellationToken ct = default);
    Task<SystemHealthStatus> GetSystemHealthAsync(CancellationToken ct = default);
}

public class ReconciliationResult
{
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public int DevicesProcessed { get; set; }
    public int SimsProcessed { get; set; }
    public int NewLinksCreated { get; set; }
    public int LinksUpdated { get; set; }
    public int DuplicateIccidsFound { get; set; }
    public int UnmatchedSims { get; set; }
    public int OrphanedDevices { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class SystemHealthStatus
{
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public bool OverallHealthy { get; set; }
    public List<ProviderHealth> ProviderHealth { get; set; } = new();
    public QueueHealth QueueHealth { get; set; } = new();
    public DatabaseHealth DatabaseHealth { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ProviderHealth
{
    public string ProviderName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public DateTime LastSuccessfulSync { get; set; }
    public TimeSpan SyncLatency { get; set; }
    public string? LastError { get; set; }
    public int ErrorCount24h { get; set; }
}

public class QueueHealth
{
    public int PendingJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int FailedJobs { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
}

public class DatabaseHealth
{
    public bool IsConnected { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public int ActiveConnections { get; set; }
}