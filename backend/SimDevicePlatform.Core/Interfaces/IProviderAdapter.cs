namespace SimDevicePlatform.Core.Interfaces;

public interface IProviderAdapter
{
    string ProviderName { get; }
    Task<ProviderInventory> FetchInventoryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DeviceHeartbeat>> FetchLastSeenAsync(IEnumerable<string> ids, CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}

public interface ISimPlatformAdapter : IProviderAdapter
{
    Task<IReadOnlyList<SimRecord>> FetchSimsAsync(CancellationToken ct = default);
    Task UpdateSimDescriptionAsync(string iccid, string description, CancellationToken ct = default);
    Task UpdateSimTagsAsync(string iccid, string[] tags, CancellationToken ct = default);
}

public interface IFotaAdapter : IProviderAdapter
{
    Task<IReadOnlyList<FirmwareVersion>> FetchFirmwareVersionsAsync(CancellationToken ct = default);
    Task<string> ScheduleFirmwareUpdateAsync(string deviceId, string targetVersion, CancellationToken ct = default);
    Task<FotaJobStatus> GetJobStatusAsync(string jobId, CancellationToken ct = default);
}

public class ProviderInventory
{
    public List<DeviceRecord> Devices { get; set; } = new();
    public List<SimRecord> Sims { get; set; } = new();
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public bool IsComplete { get; set; }
    public string? Error { get; set; }
}

public class DeviceRecord
{
    public string DeviceId { get; set; } = string.Empty;
    public DeviceOem Oem { get; set; }
    public string? Model { get; set; }
    public string? Imei { get; set; }
    public string? Serial { get; set; }
    public string? Account { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ActiveTo { get; set; }
    public string? ProviderRef { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SimRecord
{
    public string Iccid { get; set; } = string.Empty;
    public string? Msisdn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Carrier { get; set; }
    public string? AccountId { get; set; }
    public string? Description { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DeviceHeartbeat
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public string? Status { get; set; }
}

public class FirmwareVersion
{
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public bool IsStable { get; set; }
}