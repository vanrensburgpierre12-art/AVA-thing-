import { UnifiedDeviceViewFilter, UnifiedDeviceViewResult, ReconciliationResult, SystemHealthStatus, Report, ReportType } from '../types';

const API_BASE_URL = '/api';

class ApiClient {
  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  // Devices
  async getDevices(filters: UnifiedDeviceViewFilter): Promise<UnifiedDeviceViewResult> {
    const params = new URLSearchParams();
    
    if (filters.searchQuery) params.append('q', filters.searchQuery);
    if (filters.oem) params.append('oem', filters.oem);
    if (filters.status) params.append('status', filters.status);
    if (filters.account) params.append('account', filters.account);
    if (filters.hasAsset !== undefined) params.append('hasAsset', filters.hasAsset.toString());
    if (filters.hasSim !== undefined) params.append('hasSim', filters.hasSim.toString());
    params.append('page', filters.page.toString());
    params.append('pageSize', filters.pageSize.toString());

    return this.request<UnifiedDeviceViewResult>(`/devices?${params.toString()}`);
  }

  async linkDeviceToSim(deviceId: string, iccid: string, source: string, confidence: number): Promise<void> {
    return this.request<void>('/devices/link-sim', {
      method: 'POST',
      body: JSON.stringify({ deviceId, iccid, source, confidence }),
    });
  }

  async linkDeviceToAsset(assetId: string, deviceId: string, matchBasis: string): Promise<void> {
    return this.request<void>('/devices/link-asset', {
      method: 'POST',
      body: JSON.stringify({ assetId, deviceId, matchBasis }),
    });
  }

  async unlinkDeviceFromSim(deviceId: string, iccid: string): Promise<void> {
    return this.request<void>('/devices/unlink-sim', {
      method: 'DELETE',
      body: JSON.stringify({ deviceId, iccid }),
    });
  }

  async unlinkDeviceFromAsset(assetId: string, deviceId: string): Promise<void> {
    return this.request<void>('/devices/unlink-asset', {
      method: 'DELETE',
      body: JSON.stringify({ assetId, deviceId }),
    });
  }

  // Reconciliation
  async runReconciliation(forceRefresh: boolean = false): Promise<ReconciliationResult> {
    return this.request<ReconciliationResult>(`/reconcile/run?forceRefresh=${forceRefresh}`, {
      method: 'POST',
    });
  }

  async getSystemHealth(): Promise<SystemHealthStatus> {
    return this.request<SystemHealthStatus>('/reconcile/health');
  }

  // Reports
  async generateReport(type: ReportType): Promise<Report> {
    return this.request<Report>('/reports/generate', {
      method: 'POST',
      body: JSON.stringify({ type }),
    });
  }

  async getReportTypes(): Promise<Array<{ value: string; name: string; description: string }>> {
    return this.request<Array<{ value: string; name: string; description: string }>>('/reports/types');
  }

  async downloadReport(reportId: string): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/reports/${reportId}/download`);
    if (!response.ok) {
      throw new Error(`Failed to download report: ${response.status} ${response.statusText}`);
    }
    return response.blob();
  }
}

export const api = new ApiClient();