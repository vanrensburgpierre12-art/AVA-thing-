using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("sims")]
public class Sim
{
    [Key]
    [Column("iccid")]
    public string Iccid { get; set; } = string.Empty;
    
    [Column("msisdn")]
    public string? Msisdn { get; set; }
    
    [Column("status")]
    public string Status { get; set; } = string.Empty;
    
    [Column("carrier")]
    public string? Carrier { get; set; }
    
    [Column("account_id")]
    public string? AccountId { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    [Column("last_synced_at")]
    public DateTime LastSyncedAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<DeviceSimLink> DeviceSimLinks { get; set; } = new List<DeviceSimLink>();
}