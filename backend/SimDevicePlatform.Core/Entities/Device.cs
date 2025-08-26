using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("devices")]
public class Device
{
    [Key]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Column("oem")]
    public DeviceOem Oem { get; set; }
    
    [Column("model")]
    public string? Model { get; set; }
    
    [Column("imei")]
    public string? Imei { get; set; }
    
    [Column("serial")]
    public string? Serial { get; set; }
    
    [Column("last_seen_at")]
    public DateTime? LastSeenAt { get; set; }
    
    [Column("account")]
    public string? Account { get; set; }
    
    [Column("status")]
    public DeviceStatus Status { get; set; }
    
    [Column("active_to")]
    public DateTime? ActiveTo { get; set; }
    
    [Column("provider_ref")]
    public string? ProviderRef { get; set; }
    
    [Column("last_synced_at")]
    public DateTime LastSyncedAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<DeviceSimLink> DeviceSimLinks { get; set; } = new List<DeviceSimLink>();
    public virtual ICollection<AssetDeviceLink> AssetDeviceLinks { get; set; } = new List<AssetDeviceLink>();
}

public enum DeviceOem
{
    Unknown = 0,
    DigitalMatter = 1,
    Teltonika = 2
}

public enum DeviceStatus
{
    Active = 1,
    Inactive = 2
}