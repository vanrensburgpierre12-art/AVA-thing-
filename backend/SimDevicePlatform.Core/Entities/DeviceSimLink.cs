using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("links_device_sim")]
public class DeviceSimLink
{
    [Key]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Key]
    [Column("iccid")]
    public string Iccid { get; set; } = string.Empty;
    
    [Column("confidence")]
    public float Confidence { get; set; }
    
    [Column("source")]
    public LinkSource Source { get; set; }
    
    [Column("first_seen_at")]
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    
    [Column("last_seen_at")]
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
    
    [ForeignKey("Iccid")]
    public virtual Sim Sim { get; set; } = null!;
}

public enum LinkSource
{
    Iccid = 1,
    Imei = 2,
    Serial = 3
}