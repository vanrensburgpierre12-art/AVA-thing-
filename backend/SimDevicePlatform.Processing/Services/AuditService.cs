using Microsoft.EntityFrameworkCore;
using SimDevicePlatform.Core.Entities;
using SimDevicePlatform.Core.Interfaces;
using SimDevicePlatform.Infrastructure.Data;
using System.Text.Json;

namespace SimDevicePlatform.Processing.Services;

public class AuditService : IAuditService
{
    private readonly SimDeviceDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(SimDeviceDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogEventAsync(string action, string subjectType, string subjectId, object? payload = null, string? actor = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditEvent = new AuditEvent
        {
            Action = action,
            SubjectType = subjectType,
            SubjectId = subjectId,
            Payload = payload != null ? JsonSerializer.SerializeToDocument(payload) : null,
            Actor = actor ?? GetCurrentActor(),
            IpAddress = ipAddress ?? GetCurrentIpAddress(),
            UserAgent = userAgent ?? GetCurrentUserAgent()
        };

        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditEvent>> GetEventsAsync(string? subjectType = null, string? subjectId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50)
    {
        var query = _context.AuditEvents.AsQueryable();

        if (!string.IsNullOrEmpty(subjectType))
            query = query.Where(ae => ae.SubjectType == subjectType);

        if (!string.IsNullOrEmpty(subjectId))
            query = query.Where(ae => ae.SubjectId == subjectId);

        if (from.HasValue)
            query = query.Where(ae => ae.At >= from.Value);

        if (to.HasValue)
            query = query.Where(ae => ae.At <= to.Value);

        return await query
            .OrderByDescending(ae => ae.At)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private string GetCurrentActor()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.Identity.Name ?? "authenticated_user";
        }
        return "system";
    }

    private string? GetCurrentIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Connection?.RemoteIpAddress != null)
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }
        return null;
    }

    private string? GetCurrentUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers.UserAgent.ToString();
    }
}