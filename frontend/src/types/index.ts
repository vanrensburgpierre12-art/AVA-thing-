export interface UnifiedDeviceView {
  deviceId: string;
  oem: 'Unknown' | 'DigitalMatter' | 'Teltonika';
  model?: string;
  imei?: string;
  serial?: string;
  iccid?: string;
  msisdn?: string;
  assetId?: string;
  assetName?: string;
  account?: string;
  status: 'Active' | 'Inactive';
  activeTo?: string;
  lastSeenAt?: string;
  tags?: string[];
  confidence?: number;
  source?: 'Iccid' | 'Imei' | 'Serial';
  linkFirstSeenAt?: string;
  linkLastSeenAt?: string;
  lastSyncedAt: string;
}

export interface UnifiedDeviceViewFilter {
  searchQuery?: string;
  oem?: 'Unknown' | 'DigitalMatter' | 'Teltonika';
  status?: 'Active' | 'Inactive';
  account?: string;
  hasAsset?: boolean;
  hasSim?: boolean;
  page: number;
  pageSize: number;
}

export interface UnifiedDeviceViewResult {
  devices: UnifiedDeviceView[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ReconciliationResult {
  startedAt: string;
  completedAt?: string;
  isSuccess: boolean;
  error?: string;
  devicesProcessed: number;
  simsProcessed: number;
  newLinksCreated: number;
  linksUpdated: number;
  duplicateIccidsFound: number;
  unmatchedSims: number;
  orphanedDevices: number;
  metrics: Record<string, any>;
}

export interface SystemHealthStatus {
  checkedAt: string;
  overallHealthy: boolean;
  providerHealth: ProviderHealth[];
  queueHealth: QueueHealth;
  databaseHealth: DatabaseHealth;
  warnings: string[];
  errors: string[];
}

export interface ProviderHealth {
  providerName: string;
  isHealthy: boolean;
  lastSuccessfulSync: string;
  syncLatency: string;
  lastError?: string;
  errorCount24h: number;
}

export interface QueueHealth {
  pendingJobs: number;
  processingJobs: number;
  failedJobs: number;
  averageProcessingTime: string;
}

export interface DatabaseHealth {
  isConnected: boolean;
  responseTime: string;
  activeConnections: number;
}

export interface Report {
  reportId: string;
  type: ReportType;
  generatedAt: string;
  path: string;
  rowCount: number;
  fileSizeBytes: number;
  status: ReportStatus;
  error?: string;
  createdAt: string;
}

export type ReportType = 
  | 'ActiveLinkedDevices'
  | 'InactiveDevices'
  | 'SimButNoAsset'
  | 'AssetButNoSim'
  | 'NoLinkageOrphaned'
  | 'UnmatchedSims';

export type ReportStatus = 'Pending' | 'Generating' | 'Completed' | 'Failed';

export interface AuditEvent {
  id: string;
  at: string;
  actor: string;
  action: string;
  subjectType: string;
  subjectId: string;
  payload?: any;
  ipAddress?: string;
  userAgent?: string;
}