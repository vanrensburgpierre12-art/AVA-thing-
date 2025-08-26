-- Unified Device View
-- This view provides a comprehensive view of all devices with their SIM and asset linkages
CREATE OR REPLACE VIEW v_unified_device_view AS
SELECT 
    d.device_id,
    d.oem::text as oem,
    d.model,
    d.imei,
    d.serial,
    dsl.iccid,
    s.msisdn,
    adl.asset_id,
    a.name as asset_name,
    d.account,
    d.status::text as status,
    d.active_to,
    d.last_seen_at,
    COALESCE(d.tags, ARRAY[]::text[]) as tags,
    dsl.confidence,
    dsl.source::text as source,
    dsl.first_seen_at as link_first_seen_at,
    dsl.last_seen_at as link_last_seen_at,
    d.last_synced_at
FROM devices d
LEFT JOIN links_device_sim dsl ON d.device_id = dsl.device_id
LEFT JOIN sims s ON dsl.iccid = s.iccid
LEFT JOIN links_asset_device adl ON d.device_id = adl.device_id
LEFT JOIN assets a ON adl.asset_id = a.asset_id
WHERE d.deleted_at IS NULL;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_device_id ON v_unified_device_view(device_id);
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_iccid ON v_unified_device_view(iccid);
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_asset_id ON v_unified_device_view(asset_id);
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_account ON v_unified_device_view(account);
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_status ON v_unified_device_view(status);
CREATE INDEX IF NOT EXISTS IX_v_unified_device_view_oem ON v_unified_device_view(oem);