namespace SimDevicePlatform.Core.Models;

public class UnifiedDeviceView
{
    public string DeviceId { get; set; } = string.Empty;
    public DeviceOem Oem { get; set; }
    public string? Model { get; set; }
    public string? Imei { get; set; }
    public string? Serial { get; set; }
    public string? Iccid { get; set; }
    public string? Msisdn { get; set; }
    public string? AssetId { get; set; }
    public string? AssetName { get; set; }
    public string? Account { get; set; }
    public DeviceStatus Status { get; set; }
    public DateTime? ActiveTo { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public float? Confidence { get; set; }
    public LinkSource? Source { get; set; }
    public DateTime? LinkFirstSeenAt { get; set; }
    public DateTime? LinkLastSeenAt { get; set; }
    public DateTime LastSyncedAt { get; set; }
}

public class UnifiedDeviceViewFilter
{
    public string? SearchQuery { get; set; }
    public DeviceOem? Oem { get; set; }
    public DeviceStatus? Status { get; set; }
    public string? Account { get; set; }
    public bool? HasAsset { get; set; }
    public bool? HasSim { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class UnifiedDeviceViewResult
{
    public List<UnifiedDeviceView> Devices { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}