using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SimDevicePlatform.Core.Entities;

[Table("audit_events")]
public class AuditEvent
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("at")]
    public DateTime At { get; set; } = DateTime.UtcNow;
    
    [Column("actor")]
    public string Actor { get; set; } = string.Empty;
    
    [Column("action")]
    public string Action { get; set; } = string.Empty;
    
    [Column("subject_type")]
    public string SubjectType { get; set; } = string.Empty;
    
    [Column("subject_id")]
    public string SubjectId { get; set; } = string.Empty;
    
    [Column("payload")]
    public JsonDocument? Payload { get; set; }
    
    [Column("ip_address")]
    public string? IpAddress { get; set; }
    
    [Column("user_agent")]
    public string? UserAgent { get; set; }
}