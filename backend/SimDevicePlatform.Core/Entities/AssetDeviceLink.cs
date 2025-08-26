using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("links_asset_device")]
public class AssetDeviceLink
{
    [Key]
    [Column("asset_id")]
    public string AssetId { get; set; } = string.Empty;
    
    [Key]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Column("match_basis")]
    public MatchBasis MatchBasis { get; set; }
    
    [Column("first_seen_at")]
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    
    [Column("last_seen_at")]
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("AssetId")]
    public virtual Asset Asset { get; set; } = null!;
    
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

public enum MatchBasis
{
    Serial = 1,
    Imei = 2,
    Manual = 3
}