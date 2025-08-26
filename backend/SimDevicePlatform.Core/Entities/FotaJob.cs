using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("fota_jobs")]
public class FotaJob
{
    [Key]
    [Column("job_id")]
    public Guid JobId { get; set; } = Guid.NewGuid();
    
    [Column("oem")]
    public DeviceOem Oem { get; set; }
    
    [Column("target_version")]
    public string TargetVersion { get; set; } = string.Empty;
    
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;
    
    [Column("status")]
    public FotaJobStatus Status { get; set; }
    
    [Column("queued_at")]
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    
    [Column("started_at")]
    public DateTime? StartedAt { get; set; }
    
    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }
    
    [Column("provider_job_ref")]
    public string? ProviderJobRef { get; set; }
    
    [Column("error")]
    public string? Error { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

public enum FotaJobStatus
{
    Queued = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}