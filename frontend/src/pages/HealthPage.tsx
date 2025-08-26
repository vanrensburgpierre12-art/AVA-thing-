import { useQuery } from '@tanstack/react-query';
import { Activity, CheckCircle, XCircle, AlertTriangle, Database, Clock, RefreshCw } from 'lucide-react';
import { api } from '../lib/api';
import { SystemHealthStatus } from '../types';

export default function HealthPage() {
  const { data: health, isLoading, error, refetch } = useQuery({
    queryKey: ['systemHealth'],
    queryFn: api.getSystemHealth,
    refetchInterval: 30000, // Refresh every 30 seconds
  });

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        <p className="mt-2 text-gray-600">Loading system health...</p>
      </div>
    );
  }

  if (error || !health) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 text-lg font-medium">Error loading system health</div>
        <div className="text-gray-500 mt-2">Please try again later</div>
        <button
          onClick={() => refetch()}
          className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Retry
        </button>
      </div>
    );
  }

  const overallStatus = health.overallHealthy ? 'Healthy' : 'Unhealthy';
  const overallStatusColor = health.overallHealthy ? 'text-green-600' : 'text-red-600';
  const overallStatusBg = health.overallHealthy ? 'bg-green-100' : 'bg-red-100';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">System Health</h1>
          <p className="mt-2 text-sm text-gray-700">
            Monitor the health and status of all system components
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <button
            onClick={() => refetch()}
            className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </button>
        </div>
      </div>

      {/* Overall Status */}
      <div className={`rounded-lg border p-6 ${overallStatusBg}`}>
        <div className="flex items-center">
          {health.overallHealthy ? (
            <CheckCircle className="h-8 w-8 text-green-600" />
          ) : (
            <XCircle className="h-8 w-8 text-red-600" />
          )}
          <div className="ml-4">
            <h2 className={`text-lg font-medium ${overallStatusColor}`}>
              System Status: {overallStatus}
            </h2>
            <p className="text-sm text-gray-600">
              Last checked: {new Date(health.checkedAt).toLocaleString()}
            </p>
          </div>
        </div>
      </div>

      {/* Provider Health */}
      <div className="bg-white rounded-lg border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">Provider Health</h3>
        </div>
        <div className="divide-y divide-gray-200">
          {health.providerHealth.map((provider) => (
            <div key={provider.providerName} className="px-6 py-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  {provider.isHealthy ? (
                    <CheckCircle className="h-5 w-5 text-green-500" />
                  ) : (
                    <XCircle className="h-5 w-5 text-red-500" />
                  )}
                  <div className="ml-3">
                    <p className="text-sm font-medium text-gray-900">{provider.providerName}</p>
                    <p className="text-sm text-gray-500">
                      Last sync: {new Date(provider.lastSuccessfulSync).toLocaleString()}
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <div className="text-sm text-gray-900">
                    Latency: {provider.syncLatency}
                  </div>
                  {provider.errorCount24h > 0 && (
                    <div className="text-sm text-red-600">
                      {provider.errorCount24h} errors in 24h
                    </div>
                  )}
                </div>
              </div>
              {provider.lastError && (
                <div className="mt-2 p-3 bg-red-50 rounded-md">
                  <p className="text-sm text-red-800">
                    <strong>Last Error:</strong> {provider.lastError}
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Queue Health */}
      <div className="bg-white rounded-lg border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">Queue Health</h3>
        </div>
        <div className="px-6 py-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="text-center">
              <div className="text-2xl font-bold text-blue-600">{health.queueHealth.pendingJobs}</div>
              <div className="text-sm text-gray-500">Pending Jobs</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-yellow-600">{health.queueHealth.processingJobs}</div>
              <div className="text-sm text-gray-500">Processing Jobs</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-red-600">{health.queueHealth.failedJobs}</div>
              <div className="text-sm text-gray-500">Failed Jobs</div>
            </div>
          </div>
          <div className="mt-4 text-center">
            <div className="text-sm text-gray-500">
              Average Processing Time: {health.queueHealth.averageProcessingTime}
            </div>
          </div>
        </div>
      </div>

      {/* Database Health */}
      <div className="bg-white rounded-lg border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">Database Health</h3>
        </div>
        <div className="px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              {health.databaseHealth.isConnected ? (
                <CheckCircle className="h-5 w-5 text-green-500" />
              ) : (
                <XCircle className="h-5 w-5 text-red-500" />
              )}
              <div className="ml-3">
                <p className="text-sm font-medium text-gray-900">
                  Connection: {health.databaseHealth.isConnected ? 'Connected' : 'Disconnected'}
                </p>
                <p className="text-sm text-gray-500">
                  Response Time: {health.databaseHealth.responseTime}
                </p>
              </div>
            </div>
            <div className="text-right">
              <div className="text-sm text-gray-900">
                Active Connections: {health.databaseHealth.activeConnections}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Warnings and Errors */}
      {(health.warnings.length > 0 || health.errors.length > 0) && (
        <div className="space-y-4">
          {health.warnings.length > 0 && (
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
              <div className="flex">
                <AlertTriangle className="h-5 w-5 text-yellow-400" />
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-yellow-800">Warnings</h3>
                  <div className="mt-2 text-sm text-yellow-700">
                    <ul className="list-disc pl-5 space-y-1">
                      {health.warnings.map((warning, index) => (
                        <li key={index}>{warning}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          )}

          {health.errors.length > 0 && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <div className="flex">
                <XCircle className="h-5 w-5 text-red-400" />
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800">Errors</h3>
                  <div className="mt-2 text-sm text-red-700">
                    <ul className="list-disc pl-5 space-y-1">
                      {health.errors.map((error, index) => (
                        <li key={index}>{error}</li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}