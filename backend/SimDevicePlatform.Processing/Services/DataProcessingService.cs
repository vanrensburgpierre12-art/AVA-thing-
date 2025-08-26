using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimDevicePlatform.Core.Entities;
using SimDevicePlatform.Core.Interfaces;
using SimDevicePlatform.Core.Models;
using SimDevicePlatform.Infrastructure.Data;

namespace SimDevicePlatform.Processing.Services;

public class DataProcessingService : IDataProcessingService
{
    private readonly SimDeviceDbContext _context;
    private readonly ILogger<DataProcessingService> _logger;
    private readonly IEnumerable<IProviderAdapter> _providers;
    private readonly IAuditService _auditService;

    public DataProcessingService(
        SimDeviceDbContext context,
        ILogger<DataProcessingService> logger,
        IEnumerable<IProviderAdapter> providers,
        IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _providers = providers;
        _auditService = auditService;
    }

    public async Task<ReconciliationResult> RunReconciliationAsync(bool forceRefresh = false, CancellationToken ct = default)
    {
        var result = new ReconciliationResult();
        _logger.LogInformation("Starting reconciliation process. Force refresh: {ForceRefresh}", forceRefresh);

        try
        {
            // Fetch data from all providers
            var providerInventories = new List<ProviderInventory>();
            foreach (var provider in _providers)
            {
                try
                {
                    var inventory = await provider.FetchInventoryAsync(ct);
                    providerInventories.Add(inventory);
                    _logger.LogInformation("Fetched inventory from {Provider}: {DeviceCount} devices, {SimCount} SIMs", 
                        provider.ProviderName, inventory.Devices.Count, inventory.Sims.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch inventory from provider {Provider}", provider.ProviderName);
                    result.Errors.Add($"Provider {provider.ProviderName}: {ex.Message}");
                }
            }

            // Process devices
            var allDevices = providerInventories.SelectMany(pi => pi.Devices).ToList();
            result.DevicesProcessed = allDevices.Count;

            foreach (var deviceRecord in allDevices)
            {
                await ProcessDeviceAsync(deviceRecord, result, ct);
            }

            // Process SIMs
            var allSims = providerInventories.SelectMany(pi => pi.Sims).ToList();
            result.SimsProcessed = allSims.Count;

            foreach (var simRecord in allSims)
            {
                await ProcessSimAsync(simRecord, result, ct);
            }

            // Find duplicates and unmatched items
            await FindDuplicatesAndUnmatchedAsync(result, ct);

            result.CompletedAt = DateTime.UtcNow;
            result.IsSuccess = true;

            _logger.LogInformation("Reconciliation completed successfully. Processed {DeviceCount} devices, {SimCount} SIMs", 
                result.DevicesProcessed, result.SimsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconciliation failed");
            result.Error = ex.Message;
            result.IsSuccess = false;
        }

        return result;
    }

    public async Task<UnifiedDeviceViewResult> GetUnifiedDeviceViewAsync(UnifiedDeviceViewFilter filter, CancellationToken ct = default)
    {
        var query = _context.Set<UnifiedDeviceView>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            var searchTerm = filter.SearchQuery.ToLower();
            query = query.Where(d => 
                (d.DeviceId != null && d.DeviceId.ToLower().Contains(searchTerm)) ||
                (d.Imei != null && d.Imei.ToLower().Contains(searchTerm)) ||
                (d.Serial != null && d.Serial.ToLower().Contains(searchTerm)) ||
                (d.Iccid != null && d.Iccid.ToLower().Contains(searchTerm)) ||
                (d.AssetName != null && d.AssetName.ToLower().Contains(searchTerm)) ||
                (d.Account != null && d.Account.ToLower().Contains(searchTerm))
            );
        }

        if (filter.Oem.HasValue)
        {
            query = query.Where(d => d.Oem == filter.Oem.Value.ToString());
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(d => d.Status == filter.Status.Value.ToString());
        }

        if (!string.IsNullOrEmpty(filter.Account))
        {
            query = query.Where(d => d.Account == filter.Account);
        }

        if (filter.HasAsset.HasValue)
        {
            if (filter.HasAsset.Value)
                query = query.Where(d => !string.IsNullOrEmpty(d.AssetId));
            else
                query = query.Where(d => string.IsNullOrEmpty(d.AssetId));
        }

        if (filter.HasSim.HasValue)
        {
            if (filter.HasSim.Value)
                query = query.Where(d => !string.IsNullOrEmpty(d.Iccid));
            else
                query = query.Where(d => string.IsNullOrEmpty(d.Iccid));
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var devices = await query
            .OrderBy(d => d.DeviceId)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new UnifiedDeviceViewResult
        {
            Devices = devices.Select(MapToUnifiedDeviceView).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<bool> LinkDeviceToSimAsync(string deviceId, string iccid, LinkSource source, float confidence, CancellationToken ct = default)
    {
        try
        {
            var existingLink = await _context.DeviceSimLinks
                .FirstOrDefaultAsync(dsl => dsl.DeviceId == deviceId && dsl.Iccid == iccid, ct);

            if (existingLink != null)
            {
                existingLink.Confidence = confidence;
                existingLink.Source = source;
                existingLink.LastSeenAt = DateTime.UtcNow;
            }
            else
            {
                var newLink = new DeviceSimLink
                {
                    DeviceId = deviceId,
                    Iccid = iccid,
                    Confidence = confidence,
                    Source = source,
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                _context.DeviceSimLinks.Add(newLink);
            }

            await _context.SaveChangesAsync(ct);

            await _auditService.LogEventAsync("link_device_sim", "device", deviceId, new
            {
                iccid,
                source = source.ToString(),
                confidence,
                action = existingLink != null ? "updated" : "created"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link device {DeviceId} to SIM {Iccid}", deviceId, iccid);
            return false;
        }
    }

    public async Task<bool> LinkAssetToDeviceAsync(string assetId, string deviceId, MatchBasis basis, CancellationToken ct = default)
    {
        try
        {
            var existingLink = await _context.AssetDeviceLinks
                .FirstOrDefaultAsync(adl => adl.AssetId == assetId && adl.DeviceId == deviceId, ct);

            if (existingLink != null)
            {
                existingLink.MatchBasis = basis;
                existingLink.LastSeenAt = DateTime.UtcNow;
            }
            else
            {
                var newLink = new AssetDeviceLink
                {
                    AssetId = assetId,
                    DeviceId = deviceId,
                    MatchBasis = basis,
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                _context.AssetDeviceLinks.Add(newLink);
            }

            await _context.SaveChangesAsync(ct);

            await _auditService.LogEventAsync("link_asset_device", "device", deviceId, new
            {
                assetId,
                basis = basis.ToString(),
                action = existingLink != null ? "updated" : "created"
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link asset {AssetId} to device {DeviceId}", assetId, deviceId);
            return false;
        }
    }

    public async Task<bool> UnlinkDeviceFromSimAsync(string deviceId, string iccid, CancellationToken ct = default)
    {
        try
        {
            var link = await _context.DeviceSimLinks
                .FirstOrDefaultAsync(dsl => dsl.DeviceId == deviceId && dsl.Iccid == iccid, ct);

            if (link != null)
            {
                _context.DeviceSimLinks.Remove(link);
                await _context.SaveChangesAsync(ct);

                await _auditService.LogEventAsync("unlink_device_sim", "device", deviceId, new { iccid });
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink device {DeviceId} from SIM {Iccid}", deviceId, iccid);
            return false;
        }
    }

    public async Task<bool> UnlinkAssetFromDeviceAsync(string assetId, string deviceId, CancellationToken ct = default)
    {
        try
        {
            var link = await _context.AssetDeviceLinks
                .FirstOrDefaultAsync(adl => adl.AssetId == assetId && adl.DeviceId == deviceId, ct);

            if (link != null)
            {
                _context.AssetDeviceLinks.Remove(link);
                await _context.SaveChangesAsync(ct);

                await _auditService.LogEventAsync("unlink_asset_device", "device", deviceId, new { assetId });
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlink asset {AssetId} from device {DeviceId}", assetId, deviceId);
            return false;
        }
    }

    public async Task<Report> GenerateReportAsync(ReportType type, CancellationToken ct = default)
    {
        var report = new Report
        {
            Type = type,
            Status = ReportStatus.Generating,
            GeneratedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(ct);

        try
        {
            var csvData = await GenerateCsvDataAsync(type, ct);
            var filePath = await SaveCsvFileAsync(report.ReportId, type, csvData, ct);

            report.Path = filePath;
            report.RowCount = csvData.Count;
            report.FileSizeBytes = new FileInfo(filePath).Length;
            report.Status = ReportStatus.Completed;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogEventAsync("report_generated", "report", report.ReportId.ToString(), new
            {
                type = type.ToString(),
                rowCount = report.RowCount,
                fileSizeBytes = report.FileSizeBytes
            });

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportType}", type);
            report.Status = ReportStatus.Failed;
            report.Error = ex.Message;
            await _context.SaveChangesAsync(ct);
            throw;
        }
    }

    public async Task<SystemHealthStatus> GetSystemHealthAsync(CancellationToken ct = default)
    {
        var health = new SystemHealthStatus
        {
            CheckedAt = DateTime.UtcNow
        };

        // Check provider health
        foreach (var provider in _providers)
        {
            try
            {
                var isHealthy = await provider.IsHealthyAsync(ct);
                var lastSync = await GetLastSuccessfulSyncAsync(provider.ProviderName, ct);
                
                health.ProviderHealth.Add(new ProviderHealth
                {
                    ProviderName = provider.ProviderName,
                    IsHealthy = isHealthy,
                    LastSuccessfulSync = lastSync,
                    SyncLatency = DateTime.UtcNow - lastSync,
                    ErrorCount24h = await GetErrorCount24hAsync(provider.ProviderName, ct)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check health for provider {Provider}", provider.ProviderName);
                health.ProviderHealth.Add(new ProviderHealth
                {
                    ProviderName = provider.ProviderName,
                    IsHealthy = false,
                    LastSuccessfulSync = DateTime.MinValue,
                    SyncLatency = TimeSpan.MaxValue,
                    LastError = ex.Message
                });
            }
        }

        // Check database health
        try
        {
            var startTime = DateTime.UtcNow;
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            var responseTime = DateTime.UtcNow - startTime;

            health.DatabaseHealth = new DatabaseHealth
            {
                IsConnected = true,
                ResponseTime = responseTime,
                ActiveConnections = 1 // Simplified for now
            };
        }
        catch (Exception ex)
        {
            health.DatabaseHealth = new DatabaseHealth
            {
                IsConnected = false,
                ResponseTime = TimeSpan.MaxValue,
                ActiveConnections = 0
            };
            health.Errors.Add($"Database connection failed: {ex.Message}");
        }

        // Check queue health (simplified for now)
        health.QueueHealth = new QueueHealth
        {
            PendingJobs = 0,
            ProcessingJobs = 0,
            FailedJobs = 0,
            AverageProcessingTime = TimeSpan.Zero
        };

        health.OverallHealthy = health.ProviderHealth.All(p => p.IsHealthy) && 
                               health.DatabaseHealth.IsConnected && 
                               health.Errors.Count == 0;

        return health;
    }

    private async Task ProcessDeviceAsync(DeviceRecord deviceRecord, ReconciliationResult result, CancellationToken ct)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceRecord.DeviceId, ct);

        if (device == null)
        {
            device = new Device
            {
                DeviceId = deviceRecord.DeviceId,
                Oem = deviceRecord.Oem,
                Model = deviceRecord.Model,
                Imei = deviceRecord.Imei,
                Serial = deviceRecord.Serial,
                Account = deviceRecord.Account,
                Status = deviceRecord.IsActive ? DeviceStatus.Active : DeviceStatus.Inactive,
                ActiveTo = deviceRecord.ActiveTo,
                ProviderRef = deviceRecord.ProviderRef,
                LastSyncedAt = DateTime.UtcNow
            };
            _context.Devices.Add(device);
        }
        else
        {
            device.Oem = deviceRecord.Oem;
            device.Model = deviceRecord.Model;
            device.Imei = deviceRecord.Imei;
            device.Serial = deviceRecord.Serial;
            device.Account = deviceRecord.Account;
            device.Status = deviceRecord.IsActive ? DeviceStatus.Active : DeviceStatus.Inactive;
            device.ActiveTo = deviceRecord.ActiveTo;
            device.ProviderRef = deviceRecord.ProviderRef;
            device.LastSyncedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
        }

        // Try to link to asset based on serial
        if (!string.IsNullOrEmpty(deviceRecord.Serial))
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.SerialMatchHint == deviceRecord.Serial, ct);
            
            if (asset != null)
            {
                await LinkAssetToDeviceAsync(asset.AssetId, device.DeviceId, MatchBasis.Serial, ct);
            }
        }
    }

    private async Task ProcessSimAsync(SimRecord simRecord, ReconciliationResult result, CancellationToken ct)
    {
        var sim = await _context.Sims
            .FirstOrDefaultAsync(s => s.Iccid == simRecord.Iccid, ct);

        if (sim == null)
        {
            sim = new Sim
            {
                Iccid = simRecord.Iccid,
                Msisdn = simRecord.Msisdn,
                Status = simRecord.Status,
                Carrier = simRecord.Carrier,
                AccountId = simRecord.AccountId,
                Description = simRecord.Description,
                Tags = simRecord.Tags,
                LastSyncedAt = DateTime.UtcNow
            };
            _context.Sims.Add(sim);
        }
        else
        {
            sim.Msisdn = simRecord.Msisdn;
            sim.Status = simRecord.Status;
            sim.Carrier = simRecord.Carrier;
            sim.AccountId = simRecord.AccountId;
            sim.Description = simRecord.Description;
            sim.Tags = simRecord.Tags;
            sim.LastSyncedAt = DateTime.UtcNow;
            sim.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task FindDuplicatesAndUnmatchedAsync(ReconciliationResult result, CancellationToken ct)
    {
        // Find duplicate ICCIDs
        var duplicateIccids = await _context.DeviceSimLinks
            .GroupBy(dsl => dsl.Iccid)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToListAsync(ct);

        result.DuplicateIccidsFound = duplicateIccids.Count;

        // Find unmatched SIMs
        var linkedIccids = await _context.DeviceSimLinks
            .Select(dsl => dsl.Iccid)
            .Distinct()
            .ToListAsync(ct);

        var unmatchedSims = await _context.Sims
            .Where(s => !linkedIccids.Contains(s.Iccid))
            .CountAsync(ct);

        result.UnmatchedSims = unmatchedSims;

        // Find orphaned devices
        var linkedDeviceIds = await _context.DeviceSimLinks
            .Select(dsl => dsl.DeviceId)
            .Distinct()
            .ToListAsync(ct);

        var orphanedDevices = await _context.Devices
            .Where(d => !linkedDeviceIds.Contains(d.DeviceId))
            .CountAsync(ct);

        result.OrphanedDevices = orphanedDevices;
    }

    private async Task<List<Dictionary<string, object>>> GenerateCsvDataAsync(ReportType type, CancellationToken ct)
    {
        switch (type)
        {
            case ReportType.ActiveLinkedDevices:
                return await GenerateActiveLinkedDevicesReportAsync(ct);
            case ReportType.InactiveDevices:
                return await GenerateInactiveDevicesReportAsync(ct);
            case ReportType.SimButNoAsset:
                return await GenerateSimButNoAssetReportAsync(ct);
            case ReportType.AssetButNoSim:
                return await GenerateAssetButNoSimReportAsync(ct);
            case ReportType.NoLinkageOrphaned:
                return await GenerateNoLinkageOrphanedReportAsync(ct);
            case ReportType.UnmatchedSims:
                return await GenerateUnmatchedSimsReportAsync(ct);
            default:
                throw new ArgumentException($"Unknown report type: {type}");
        }
    }

    private async Task<List<Dictionary<string, object>>> GenerateActiveLinkedDevicesReportAsync(CancellationToken ct)
    {
        var devices = await _context.Set<UnifiedDeviceView>()
            .Where(d => d.Status == "Active" && !string.IsNullOrEmpty(d.Iccid) && !string.IsNullOrEmpty(d.AssetId))
            .ToListAsync(ct);

        return devices.Select(d => new Dictionary<string, object>
        {
            ["ICCID"] = d.Iccid ?? "",
            ["MSISDN"] = d.Msisdn ?? "",
            ["IMEI"] = d.Imei ?? "",
            ["Serial"] = d.Serial ?? "",
            ["OEM"] = d.Oem,
            ["Model"] = d.Model ?? "",
            ["Account"] = d.Account ?? "",
            ["AssetName"] = d.AssetName ?? "",
            ["Status"] = d.Status,
            ["LastSeen"] = d.LastSeenAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["ActiveTo"] = d.ActiveTo?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["Source"] = d.Source ?? "",
            ["Confidence"] = d.Confidence?.ToString() ?? ""
        }).ToList();
    }

    private async Task<List<Dictionary<string, object>>> GenerateInactiveDevicesReportAsync(CancellationToken ct)
    {
        var devices = await _context.Set<UnifiedDeviceView>()
            .Where(d => d.Status == "Inactive")
            .ToListAsync(ct);

        return devices.Select(d => new Dictionary<string, object>
        {
            ["DeviceID"] = d.DeviceId,
            ["OEM"] = d.Oem,
            ["Model"] = d.Model ?? "",
            ["IMEI"] = d.Imei ?? "",
            ["Serial"] = d.Serial ?? "",
            ["Account"] = d.Account ?? "",
            ["Status"] = d.Status,
            ["ActiveTo"] = d.ActiveTo?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            ["LastSeen"] = d.LastSeenAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
        }).ToList();
    }

    private async Task<List<Dictionary<string, object>>> GenerateSimButNoAssetReportAsync(CancellationToken ct)
    {
        var devices = await _context.Set<UnifiedDeviceView>()
            .Where(d => !string.IsNullOrEmpty(d.Iccid) && string.IsNullOrEmpty(d.AssetId))
            .ToListAsync(ct);

        return devices.Select(d => new Dictionary<string, object>
        {
            ["ICCID"] = d.Iccid ?? "",
            ["MSISDN"] = d.Msisdn ?? "",
            ["DeviceID"] = d.DeviceId,
            ["OEM"] = d.Oem,
            ["Model"] = d.Model ?? "",
            ["Account"] = d.Account ?? "",
            ["Status"] = d.Status
        }).ToList();
    }

    private async Task<List<Dictionary<string, object>>> GenerateAssetButNoSimReportAsync(CancellationToken ct)
    {
        var devices = await _context.Set<UnifiedDeviceView>()
            .Where(d => !string.IsNullOrEmpty(d.AssetId) && string.IsNullOrEmpty(d.Iccid))
            .ToListAsync(ct);

        return devices.Select(d => new Dictionary<string, object>
        {
            ["AssetID"] = d.AssetId ?? "",
            ["AssetName"] = d.AssetName ?? "",
            ["DeviceID"] = d.DeviceId,
            ["OEM"] = d.Oem,
            ["Model"] = d.Model ?? "",
            ["IMEI"] = d.Imei ?? "",
            ["Serial"] = d.Serial ?? "",
            ["Account"] = d.Account ?? "",
            ["Status"] = d.Status
        }).ToList();
    }

    private async Task<List<Dictionary<string, object>>> GenerateNoLinkageOrphanedReportAsync(CancellationToken ct)
    {
        var devices = await _context.Set<UnifiedDeviceView>()
            .Where(d => string.IsNullOrEmpty(d.Iccid) && string.IsNullOrEmpty(d.AssetId))
            .ToListAsync(ct);

        return devices.Select(d => new Dictionary<string, object>
        {
            ["DeviceID"] = d.DeviceId,
            ["OEM"] = d.Oem,
            ["Model"] = d.Model ?? "",
            ["IMEI"] = d.Imei ?? "",
            ["Serial"] = d.Serial ?? "",
            ["Account"] = d.Account ?? "",
            ["Status"] = d.Status,
            ["LastSeen"] = d.LastSeenAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
        }).ToList();
    }

    private async Task<List<Dictionary<string, object>>> GenerateUnmatchedSimsReportAsync(CancellationToken ct)
    {
        var linkedIccids = await _context.DeviceSimLinks
            .Select(dsl => dsl.Iccid)
            .Distinct()
            .ToListAsync(ct);

        var unmatchedSims = await _context.Sims
            .Where(s => !linkedIccids.Contains(s.Iccid))
            .ToListAsync(ct);

        return unmatchedSims.Select(s => new Dictionary<string, object>
        {
            ["ICCID"] = s.Iccid,
            ["MSISDN"] = s.Msisdn ?? "",
            ["Carrier"] = s.Carrier ?? "",
            ["Status"] = s.Status,
            ["AccountID"] = s.AccountId ?? "",
            ["Description"] = s.Description ?? "",
            ["Tags"] = string.Join(", ", s.Tags),
            ["LastSynced"] = s.LastSyncedAt.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();
    }

    private async Task<string> SaveCsvFileAsync(Guid reportId, ReportType type, List<Dictionary<string, object>> data, CancellationToken ct)
    {
        var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        Directory.CreateDirectory(reportsDir);

        var fileName = $"{type}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{reportId}.csv";
        var filePath = Path.Combine(reportsDir, fileName);

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

        if (data.Count > 0)
        {
            // Write headers
            foreach (var key in data[0].Keys)
            {
                csv.WriteField(key);
            }
            csv.NextRecord();

            // Write data
            foreach (var row in data)
            {
                foreach (var value in row.Values)
                {
                    csv.WriteField(value?.ToString() ?? "");
                }
                csv.NextRecord();
            }
        }

        return filePath;
    }

    private UnifiedDeviceView MapToUnifiedDeviceView(Infrastructure.Data.UnifiedDeviceView dbView)
    {
        return new UnifiedDeviceView
        {
            DeviceId = dbView.DeviceId,
            Oem = Enum.Parse<DeviceOem>(dbView.Oem),
            Model = dbView.Model,
            Imei = dbView.Imei,
            Serial = dbView.Serial,
            Iccid = dbView.Iccid,
            Msisdn = dbView.Msisdn,
            AssetId = dbView.AssetId,
            AssetName = dbView.AssetName,
            Account = dbView.Account,
            Status = Enum.Parse<DeviceStatus>(dbView.Status),
            ActiveTo = dbView.ActiveTo,
            LastSeenAt = dbView.LastSeenAt,
            Tags = dbView.Tags,
            Confidence = dbView.Confidence,
            Source = !string.IsNullOrEmpty(dbView.Source) ? Enum.Parse<LinkSource>(dbView.Source) : null,
            LinkFirstSeenAt = dbView.LinkFirstSeenAt,
            LinkLastSeenAt = dbView.LinkLastSeenAt,
            LastSyncedAt = dbView.LastSyncedAt
        };
    }

    private async Task<DateTime> GetLastSuccessfulSyncAsync(string providerName, CancellationToken ct)
    {
        // This would typically come from a sync history table
        // For now, return a default value
        return DateTime.UtcNow.AddHours(-1);
    }

    private async Task<int> GetErrorCount24hAsync(string providerName, CancellationToken ct)
    {
        // This would typically come from an error log table
        // For now, return 0
        return 0;
    }
}