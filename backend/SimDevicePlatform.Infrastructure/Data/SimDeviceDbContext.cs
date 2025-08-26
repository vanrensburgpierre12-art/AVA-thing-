using Microsoft.EntityFrameworkCore;
using SimDevicePlatform.Core.Entities;

namespace SimDevicePlatform.Infrastructure.Data;

public class SimDeviceDbContext : DbContext
{
    public SimDeviceDbContext(DbContextOptions<SimDeviceDbContext> options) : base(options)
    {
    }

    public DbSet<Sim> Sims => Set<Sim>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<DeviceSimLink> DeviceSimLinks => Set<DeviceSimLink>();
    public DbSet<AssetDeviceLink> AssetDeviceLinks => Set<AssetDeviceLink>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<FotaJob> FotaJobs => Set<FotaJob>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite keys
        modelBuilder.Entity<DeviceSimLink>()
            .HasKey(dsl => new { dsl.DeviceId, dsl.Iccid });

        modelBuilder.Entity<AssetDeviceLink>()
            .HasKey(adl => new { adl.AssetId, adl.DeviceId });

        // Configure relationships
        modelBuilder.Entity<DeviceSimLink>()
            .HasOne(dsl => dsl.Device)
            .WithMany(d => d.DeviceSimLinks)
            .HasForeignKey(dsl => dsl.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeviceSimLink>()
            .HasOne(dsl => dsl.Sim)
            .WithMany(s => s.DeviceSimLinks)
            .HasForeignKey(dsl => dsl.Iccid)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetDeviceLink>()
            .HasOne(adl => adl.Asset)
            .WithMany(a => a.AssetDeviceLinks)
            .HasForeignKey(adl => adl.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetDeviceLink>()
            .HasOne(adl => adl.Device)
            .WithMany(d => d.AssetDeviceLinks)
            .HasForeignKey(adl => adl.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure enums
        modelBuilder.Entity<Device>()
            .Property(d => d.Oem)
            .HasConversion<string>();

        modelBuilder.Entity<Device>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DeviceSimLink>()
            .Property(dsl => dsl.Source)
            .HasConversion<string>();

        modelBuilder.Entity<AssetDeviceLink>()
            .Property(adl => adl.MatchBasis)
            .HasConversion<string>();

        modelBuilder.Entity<FotaJob>()
            .Property(fj => fj.Oem)
            .HasConversion<string>();

        modelBuilder.Entity<FotaJob>()
            .Property(fj => fj.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Report>()
            .Property(r => r.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Report>()
            .Property(r => r.Status)
            .HasConversion<string>();

        // Configure arrays for PostgreSQL
        modelBuilder.Entity<Sim>()
            .Property(s => s.Tags)
            .HasColumnType("text[]");

        modelBuilder.Entity<Device>()
            .Property(d => d.Tags)
            .HasColumnType("text[]");

        // Configure JSON for PostgreSQL
        modelBuilder.Entity<AuditEvent>()
            .Property(ae => ae.Payload)
            .HasColumnType("jsonb");

        // Configure indexes
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Imei)
            .HasDatabaseName("IX_devices_imei");

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Serial)
            .HasDatabaseName("IX_devices_serial");

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Account)
            .HasDatabaseName("IX_devices_account");

        modelBuilder.Entity<Sim>()
            .HasIndex(s => s.Msisdn)
            .HasDatabaseName("IX_sims_msisdn");

        modelBuilder.Entity<Sim>()
            .HasIndex(s => s.AccountId)
            .HasDatabaseName("IX_sims_account_id");

        modelBuilder.Entity<AuditEvent>()
            .HasIndex(ae => ae.At)
            .HasDatabaseName("IX_audit_events_at");

        modelBuilder.Entity<AuditEvent>()
            .HasIndex(ae => new { ae.SubjectType, ae.SubjectId })
            .HasDatabaseName("IX_audit_events_subject");

        // Configure the unified view
        modelBuilder.Entity<UnifiedDeviceView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("v_unified_device_view");
        });
    }
}

// View model for the unified device view
public class UnifiedDeviceView
{
    public string DeviceId { get; set; } = string.Empty;
    public string Oem { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Imei { get; set; }
    public string? Serial { get; set; }
    public string? Iccid { get; set; }
    public string? Msisdn { get; set; }
    public string? AssetId { get; set; }
    public string? AssetName { get; set; }
    public string? Account { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActiveTo { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public float? Confidence { get; set; }
    public string? Source { get; set; }
    public DateTime? LinkFirstSeenAt { get; set; }
    public DateTime? LinkLastSeenAt { get; set; }
    public DateTime LastSyncedAt { get; set; }
}