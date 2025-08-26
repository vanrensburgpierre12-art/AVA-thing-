namespace SimDevicePlatform.Core.Interfaces;

public interface IAuditService
{
    Task LogEventAsync(string action, string subjectType, string subjectId, object? payload = null, string? actor = null, string? ipAddress = null, string? userAgent = null);
    Task<IEnumerable<AuditEvent>> GetEventsAsync(string? subjectType = null, string? subjectId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50);
}