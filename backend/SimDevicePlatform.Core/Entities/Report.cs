using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimDevicePlatform.Core.Entities;

[Table("reports")]
public class Report
{
    [Key]
    [Column("report_id")]
    public Guid ReportId { get; set; } = Guid.NewGuid();
    
    [Column("type")]
    public ReportType Type { get; set; }
    
    [Column("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    [Column("path")]
    public string Path { get; set; } = string.Empty;
    
    [Column("row_count")]
    public int RowCount { get; set; }
    
    [Column("file_size_bytes")]
    public long FileSizeBytes { get; set; }
    
    [Column("status")]
    public ReportStatus Status { get; set; }
    
    [Column("error")]
    public string? Error { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReportType
{
    ActiveLinkedDevices = 1,
    InactiveDevices = 2,
    SimButNoAsset = 3,
    AssetButNoSim = 4,
    NoLinkageOrphaned = 5,
    UnmatchedSims = 6
}

public enum ReportStatus
{
    Pending = 1,
    Generating = 2,
    Completed = 3,
    Failed = 4
}