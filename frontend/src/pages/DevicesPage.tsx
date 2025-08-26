import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Search, Filter, Download, Link, Unlink } from 'lucide-react';
import { format } from 'date-fns';
import DeviceTable from '../components/DeviceTable';
import DeviceFilters from '../components/DeviceFilters';
import { api } from '../lib/api';
import { UnifiedDeviceView, UnifiedDeviceViewFilter } from '../types';

export default function DevicesPage() {
  const [filters, setFilters] = useState<UnifiedDeviceViewFilter>({
    page: 1,
    pageSize: 50,
  });

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['devices', filters],
    queryFn: () => api.getDevices(filters),
    keepPreviousData: true,
  });

  const handleFiltersChange = (newFilters: Partial<UnifiedDeviceViewFilter>) => {
    setFilters(prev => ({ ...prev, ...newFilters, page: 1 }));
  };

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }));
  };

  if (error) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 text-lg font-medium">Error loading devices</div>
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

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Devices</h1>
          <p className="mt-2 text-sm text-gray-700">
            Unified view of all devices with SIM and asset linkages
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none space-x-3">
          <button
            onClick={() => refetch()}
            className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </button>
          <button className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700">
            <Download className="h-4 w-4 mr-2" />
            Export
          </button>
        </div>
      </div>

      {/* Filters */}
      <DeviceFilters filters={filters} onFiltersChange={handleFiltersChange} />

      {/* Results summary */}
      {data && (
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between">
            <div className="text-sm text-gray-700">
              Showing {((data.page - 1) * data.pageSize) + 1} to{' '}
              {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} devices
            </div>
            <div className="text-sm text-gray-500">
              Page {data.page} of {data.totalPages}
            </div>
          </div>
        </div>
      )}

      {/* Devices table */}
      <div className="bg-white rounded-lg border border-gray-200">
        <DeviceTable
          devices={data?.devices || []}
          isLoading={isLoading}
          onPageChange={handlePageChange}
          currentPage={filters.page}
          totalPages={data?.totalPages || 1}
        />
      </div>
    </div>
  );
}

// RefreshCw icon component
function RefreshCw({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
    </svg>
  );
}