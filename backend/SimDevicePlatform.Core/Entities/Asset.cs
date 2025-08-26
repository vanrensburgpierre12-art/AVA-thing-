using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("assets")]
public class Asset
{
    [Key]
    [Column("asset_id")]
    public string AssetId { get; set; } = string.Empty;
    
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [Column("external_ref")]
    public string? ExternalRef { get; set; }
    
    [Column("serial_match_hint")]
    public string? SerialMatchHint { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<AssetDeviceLink> AssetDeviceLinks { get; set; } = new List<AssetDeviceLink>();
}