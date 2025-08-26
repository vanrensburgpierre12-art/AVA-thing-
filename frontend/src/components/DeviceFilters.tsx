import { useState } from 'react';
import { Search, Filter, X } from 'lucide-react';
import { UnifiedDeviceViewFilter } from '../types';

interface DeviceFiltersProps {
  filters: UnifiedDeviceViewFilter;
  onFiltersChange: (filters: Partial<UnifiedDeviceViewFilter>) => void;
}

export default function DeviceFilters({ filters, onFiltersChange }: DeviceFiltersProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  const handleSearchChange = (value: string) => {
    onFiltersChange({ searchQuery: value || undefined });
  };

  const handleOemChange = (value: string) => {
    onFiltersChange({ oem: value === 'all' ? undefined : value as any });
  };

  const handleStatusChange = (value: string) => {
    onFiltersChange({ status: value === 'all' ? undefined : value as any });
  };

  const handleAccountChange = (value: string) => {
    onFiltersChange({ account: value || undefined });
  };

  const handleHasAssetChange = (value: string) => {
    onFiltersChange({ hasAsset: value === 'all' ? undefined : value === 'true' });
  };

  const handleHasSimChange = (value: string) => {
    onFiltersChange({ hasSim: value === 'all' ? undefined : value === 'true' });
  };

  const clearFilters = () => {
    onFiltersChange({
      searchQuery: undefined,
      oem: undefined,
      status: undefined,
      account: undefined,
      hasAsset: undefined,
      hasSim: undefined,
    });
  };

  const hasActiveFilters = filters.searchQuery || filters.oem || filters.status || 
                          filters.account || filters.hasAsset !== undefined || filters.hasSim !== undefined;

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      {/* Search bar */}
      <div className="flex items-center space-x-4">
        <div className="flex-1">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search devices, IMEI, serial, ICCID, asset name..."
              value={filters.searchQuery || ''}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="inline-flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
        >
          <Filter className="h-4 w-4 mr-2" />
          Filters
          {hasActiveFilters && (
            <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              Active
            </span>
          )}
        </button>
        {hasActiveFilters && (
          <button
            onClick={clearFilters}
            className="inline-flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-600 bg-white hover:bg-gray-50"
          >
            <X className="h-4 w-4 mr-2" />
            Clear
          </button>
        )}
      </div>

      {/* Advanced filters */}
      {isExpanded && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {/* OEM Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">OEM</label>
              <select
                value={filters.oem || 'all'}
                onChange={(e) => handleOemChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="all">All OEMs</option>
                <option value="DigitalMatter">Digital Matter</option>
                <option value="Teltonika">Teltonika</option>
                <option value="Unknown">Unknown</option>
              </select>
            </div>

            {/* Status Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select
                value={filters.status || 'all'}
                onChange={(e) => handleStatusChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="all">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>

            {/* Account Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Account</label>
              <input
                type="text"
                placeholder="Filter by account"
                value={filters.account || ''}
                onChange={(e) => handleAccountChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            {/* Has Asset Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Asset Linkage</label>
              <select
                value={filters.hasAsset === undefined ? 'all' : filters.hasAsset.toString()}
                onChange={(e) => handleHasAssetChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="all">All</option>
                <option value="true">Has Asset</option>
                <option value="false">No Asset</option>
              </select>
            </div>

            {/* Has SIM Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">SIM Linkage</label>
              <select
                value={filters.hasSim === undefined ? 'all' : filters.hasSim.toString()}
                onChange={(e) => handleHasSimChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="all">All</option>
                <option value="true">Has SIM</option>
                <option value="false">No SIM</option>
              </select>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}