import { useState } from 'react';
import { Shield, Search, Filter, Calendar, User, Activity } from 'lucide-react';
import { format } from 'date-fns';

export default function AuditPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedSubjectType, setSelectedSubjectType] = useState('all');
  const [dateRange, setDateRange] = useState('24h');

  // Mock audit data - in a real app this would come from the API
  const mockAuditEvents = [
    {
      id: '1',
      at: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
      actor: 'john.doe@company.com',
      action: 'link_device_sim',
      subjectType: 'device',
      subjectId: 'DEV-001',
      payload: { iccid: '89014103211118510720', source: 'iccid', confidence: 1.0 },
      ipAddress: '192.168.1.100',
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    },
    {
      id: '2',
      at: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
      actor: 'jane.smith@company.com',
      action: 'unlink_asset_device',
      subjectType: 'device',
      subjectId: 'DEV-002',
      payload: { assetId: 'ASSET-001' },
      ipAddress: '192.168.1.101',
      userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36'
    },
    {
      id: '3',
      at: new Date(Date.now() - 1000 * 60 * 60 * 24), // 1 day ago
      actor: 'system',
      action: 'reconciliation_completed',
      subjectType: 'system',
      subjectId: 'reconciliation',
      payload: { 
        devicesProcessed: 1250, 
        simsProcessed: 1200, 
        newLinksCreated: 45,
        duplicateIccidsFound: 3
      },
      ipAddress: null,
      userAgent: null
    },
    {
      id: '4',
      at: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2), // 2 days ago
      actor: 'admin@company.com',
      action: 'report_generated',
      subjectType: 'report',
      subjectId: 'REP-001',
      payload: { type: 'ActiveLinkedDevices', rowCount: 1150, fileSizeBytes: 45678 },
      ipAddress: '192.168.1.102',
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    }
  ];

  const filteredEvents = mockAuditEvents.filter(event => {
    if (searchQuery && !event.subjectId.toLowerCase().includes(searchQuery.toLowerCase())) {
      return false;
    }
    if (selectedSubjectType !== 'all' && event.subjectType !== selectedSubjectType) {
      return false;
    }
    return true;
  });

  const getActionIcon = (action: string) => {
    switch (action) {
      case 'link_device_sim':
        return '🔗';
      case 'unlink_asset_device':
        return '🔓';
      case 'reconciliation_completed':
        return '✅';
      case 'report_generated':
        return '📊';
      default:
        return '📝';
    }
  };

  const getActionDisplayName = (action: string) => {
    return action.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
  };

  const getSubjectTypeDisplayName = (type: string) => {
    switch (type) {
      case 'device':
        return 'Device';
      case 'sim':
        return 'SIM';
      case 'asset':
        return 'Asset';
      case 'report':
        return 'Report';
      case 'system':
        return 'System';
      default:
        return type.charAt(0).toUpperCase() + type.slice(1);
    }
  };

  const getTimeAgo = (date: Date) => {
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes} minutes ago`;
    
    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours} hours ago`;
    
    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) return `${diffInDays} days ago`;
    
    return format(date, 'MMM dd, yyyy');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Audit Log</h1>
          <p className="mt-2 text-sm text-gray-700">
            Track all system activities and changes for compliance and troubleshooting
          </p>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg border border-gray-200 p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search by subject ID..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
          </div>
          
          <div className="flex gap-4">
            <select
              value={selectedSubjectType}
              onChange={(e) => setSelectedSubjectType(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="all">All Types</option>
              <option value="device">Device</option>
              <option value="sim">SIM</option>
              <option value="asset">Asset</option>
              <option value="report">Report</option>
              <option value="system">System</option>
            </select>
            
            <select
              value={dateRange}
              onChange={(e) => setDateRange(e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="24h">Last 24 hours</option>
              <option value="7d">Last 7 days</option>
              <option value="30d">Last 30 days</option>
              <option value="all">All time</option>
            </select>
          </div>
        </div>
      </div>

      {/* Audit Events */}
      <div className="bg-white rounded-lg border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">
            Recent Activity ({filteredEvents.length} events)
          </h3>
        </div>
        <div className="divide-y divide-gray-200">
          {filteredEvents.map((event) => (
            <div key={event.id} className="px-6 py-4 hover:bg-gray-50">
              <div className="flex items-start space-x-4">
                <div className="flex-shrink-0">
                  <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center text-lg">
                    {getActionIcon(event.action)}
                  </div>
                </div>
                
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-gray-900">
                        {getActionDisplayName(event.action)}
                      </p>
                      <p className="text-sm text-gray-500">
                        {getSubjectTypeDisplayName(event.subjectType)}: {event.subjectId}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm text-gray-900">{getTimeAgo(event.at)}</p>
                      <p className="text-xs text-gray-500">
                        {format(event.at, 'MMM dd, yyyy HH:mm:ss')}
                      </p>
                    </div>
                  </div>
                  
                  <div className="mt-2 flex items-center space-x-4 text-sm text-gray-500">
                    <div className="flex items-center">
                      <User className="h-4 w-4 mr-1" />
                      {event.actor}
                    </div>
                    {event.ipAddress && (
                      <div className="flex items-center">
                        <Activity className="h-4 w-4 mr-1" />
                        {event.ipAddress}
                      </div>
                    )}
                  </div>
                  
                  {event.payload && (
                    <div className="mt-2 p-3 bg-gray-50 rounded-md">
                      <details className="text-sm">
                        <summary className="cursor-pointer text-gray-700 font-medium">
                          View Details
                        </summary>
                        <pre className="mt-2 text-xs text-gray-600 whitespace-pre-wrap">
                          {JSON.stringify(event.payload, null, 2)}
                        </pre>
                      </details>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Help Section */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <div className="flex">
          <Shield className="h-5 w-5 text-blue-400 mr-3 mt-0.5" />
          <div>
            <h3 className="text-sm font-medium text-blue-800">About Audit Logging</h3>
            <div className="mt-2 text-sm text-blue-700">
              <p>
                All system activities are automatically logged for security, compliance, and troubleshooting purposes. 
                The audit log includes user actions, system events, and detailed payload information. 
                Logs are retained according to your organization's data retention policies.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}