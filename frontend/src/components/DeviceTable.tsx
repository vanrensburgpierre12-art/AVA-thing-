import { useState } from 'react';
import { ChevronLeft, ChevronRight, MoreHorizontal, Link, Unlink, Eye } from 'lucide-react';
import { UnifiedDeviceView } from '../types';
import { format } from 'date-fns';

interface DeviceTableProps {
  devices: UnifiedDeviceView[];
  isLoading: boolean;
  onPageChange: (page: number) => void;
  currentPage: number;
  totalPages: number;
}

export default function DeviceTable({ 
  devices, 
  isLoading, 
  onPageChange, 
  currentPage, 
  totalPages 
}: DeviceTableProps) {
  const [selectedDevice, setSelectedDevice] = useState<UnifiedDeviceView | null>(null);

  if (isLoading) {
    return (
      <div className="p-8 text-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        <p className="mt-2 text-gray-600">Loading devices...</p>
      </div>
    );
  }

  if (devices.length === 0) {
    return (
      <div className="p-8 text-center">
        <p className="text-gray-500">No devices found matching your criteria.</p>
      </div>
    );
  }

  return (
    <div>
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Device
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                SIM
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Asset
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Last Seen
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Confidence
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {devices.map((device) => (
              <tr key={device.deviceId} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div>
                    <div className="text-sm font-medium text-gray-900">{device.deviceId}</div>
                    <div className="text-sm text-gray-500">
                      {device.oem} • {device.model || 'Unknown Model'}
                    </div>
                    {device.imei && (
                      <div className="text-xs text-gray-400">IMEI: {device.imei}</div>
                    )}
                    {device.serial && (
                      <div className="text-xs text-gray-400">Serial: {device.serial}</div>
                    )}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {device.iccid ? (
                    <div>
                      <div className="text-sm text-gray-900">{device.iccid}</div>
                      {device.msisdn && (
                        <div className="text-xs text-gray-500">MSISDN: {device.msisdn}</div>
                      )}
                      <div className="text-xs text-gray-400">
                        Source: {device.source || 'Unknown'}
                      </div>
                    </div>
                  ) : (
                    <span className="text-gray-400 text-sm">No SIM</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {device.assetName ? (
                    <div>
                      <div className="text-sm text-gray-900">{device.assetName}</div>
                      <div className="text-xs text-gray-500">ID: {device.assetId}</div>
                    </div>
                  ) : (
                    <span className="text-gray-400 text-sm">No Asset</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                    device.status === 'Active' 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {device.status}
                  </span>
                  {device.account && (
                    <div className="text-xs text-gray-500 mt-1">{device.account}</div>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {device.lastSeenAt ? (
                    <div>
                      <div>{format(new Date(device.lastSeenAt), 'MMM dd, yyyy')}</div>
                      <div className="text-xs text-gray-400">
                        {format(new Date(device.lastSeenAt), 'HH:mm:ss')}
                      </div>
                    </div>
                  ) : (
                    <span className="text-gray-400">Never</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {device.confidence !== undefined ? (
                    <div className="flex items-center">
                      <div className="w-16 bg-gray-200 rounded-full h-2 mr-2">
                        <div 
                          className="bg-blue-600 h-2 rounded-full" 
                          style={{ width: `${device.confidence * 100}%` }}
                        ></div>
                      </div>
                      <span className="text-sm text-gray-600">
                        {Math.round(device.confidence * 100)}%
                      </span>
                    </div>
                  ) : (
                    <span className="text-gray-400 text-sm">-</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <div className="flex items-center space-x-2">
                    <button
                      onClick={() => setSelectedDevice(device)}
                      className="text-blue-600 hover:text-blue-900 p-1"
                      title="View Details"
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                    {device.iccid ? (
                      <button
                        className="text-red-600 hover:text-red-900 p-1"
                        title="Unlink SIM"
                      >
                        <Unlink className="h-4 w-4" />
                      </button>
                    ) : (
                      <button
                        className="text-green-600 hover:text-green-900 p-1"
                        title="Link SIM"
                      >
                        <Link className="h-4 w-4" />
                      </button>
                    )}
                    <button className="text-gray-400 hover:text-gray-600 p-1">
                      <MoreHorizontal className="h-4 w-4" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
          <div className="flex-1 flex justify-between sm:hidden">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage === totalPages}
              className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
          <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-700">
                Page <span className="font-medium">{currentPage}</span> of{' '}
                <span className="font-medium">{totalPages}</span>
              </p>
            </div>
            <div>
              <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                <button
                  onClick={() => onPageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                  className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <ChevronLeft className="h-5 w-5" />
                </button>
                
                {/* Page numbers */}
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  let pageNum;
                  if (totalPages <= 5) {
                    pageNum = i + 1;
                  } else if (currentPage <= 3) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 2) {
                    pageNum = totalPages - 4 + i;
                  } else {
                    pageNum = currentPage - 2 + i;
                  }
                  
                  return (
                    <button
                      key={pageNum}
                      onClick={() => onPageChange(pageNum)}
                      className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                        pageNum === currentPage
                          ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                          : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                      }`}
                    >
                      {pageNum}
                    </button>
                  );
                })}
                
                <button
                  onClick={() => onPageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <ChevronRight className="h-5 w-5" />
                </button>
              </nav>
            </div>
          </div>
        </div>
      )}

      {/* Device Details Modal */}
      {selectedDevice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Device Details</h3>
              <div className="space-y-3">
                <div>
                  <label className="text-sm font-medium text-gray-700">Device ID</label>
                  <p className="text-sm text-gray-900">{selectedDevice.deviceId}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">OEM</label>
                  <p className="text-sm text-gray-900">{selectedDevice.oem}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Model</label>
                  <p className="text-sm text-gray-900">{selectedDevice.model || 'Unknown'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">IMEI</label>
                  <p className="text-sm text-gray-900">{selectedDevice.imei || 'Not available'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Serial</label>
                  <p className="text-sm text-gray-900">{selectedDevice.serial || 'Not available'}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Status</label>
                  <p className="text-sm text-gray-900">{selectedDevice.status}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Last Synced</label>
                  <p className="text-sm text-gray-900">
                    {format(new Date(selectedDevice.lastSyncedAt), 'MMM dd, yyyy HH:mm:ss')}
                  </p>
                </div>
              </div>
              <div className="mt-6 flex justify-end">
                <button
                  onClick={() => setSelectedDevice(null)}
                  className="px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}