import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { FileText, Download, Play, Clock, CheckCircle, XCircle, AlertTriangle } from 'lucide-react';
import { api } from '../lib/api';
import { ReportType } from '../types';

export default function ReportsPage() {
  const [selectedReportType, setSelectedReportType] = useState<ReportType>('ActiveLinkedDevices');
  const queryClient = useQueryClient();

  const { data: reportTypes, isLoading: typesLoading } = useQuery({
    queryKey: ['reportTypes'],
    queryFn: api.getReportTypes,
  });

  const generateReportMutation = useMutation({
    mutationFn: (type: ReportType) => api.generateReport(type),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reports'] });
    },
  });

  const handleGenerateReport = () => {
    generateReportMutation.mutate(selectedReportType);
  };

  const getReportTypeIcon = (type: ReportType) => {
    switch (type) {
      case 'ActiveLinkedDevices':
        return '📱';
      case 'InactiveDevices':
        return '⏸️';
      case 'SimButNoAsset':
        return '📋';
      case 'AssetButNoSim':
        return '🔗';
      case 'NoLinkageOrphaned':
        return '❓';
      case 'UnmatchedSims':
        return '📞';
      default:
        return '📄';
    }
  };

  const getReportTypeDescription = (type: ReportType) => {
    switch (type) {
      case 'ActiveLinkedDevices':
        return 'Devices with both SIM and asset linkages';
      case 'InactiveDevices':
        return 'Devices marked as inactive';
      case 'SimButNoAsset':
        return 'Devices linked to SIM but missing asset';
      case 'AssetButNoSim':
        return 'Devices linked to asset but missing SIM';
      case 'NoLinkageOrphaned':
        return 'Devices without SIM or asset linkages';
      case 'UnmatchedSims':
        return 'SIMs not linked to any device';
      default:
        return 'Unknown report type';
    }
  };

  if (typesLoading) {
    return (
      <div className="text-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        <p className="mt-2 text-gray-600">Loading report types...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Reports</h1>
          <p className="mt-2 text-sm text-gray-700">
            Generate and download CSV reports for data analysis
          </p>
        </div>
      </div>

      {/* Report Generation */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Generate New Report</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Report Type
            </label>
            <select
              value={selectedReportType}
              onChange={(e) => setSelectedReportType(e.target.value as ReportType)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {reportTypes?.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.name}
                </option>
              ))}
            </select>
          </div>
          
          <div className="bg-gray-50 rounded-lg p-4">
            <div className="flex items-start">
              <span className="text-2xl mr-3">
                {getReportTypeIcon(selectedReportType)}
              </span>
              <div>
                <h4 className="font-medium text-gray-900">
                  {reportTypes?.find(t => t.value === selectedReportType)?.name}
                </h4>
                <p className="text-sm text-gray-600 mt-1">
                  {getReportTypeDescription(selectedReportType)}
                </p>
              </div>
            </div>
          </div>

          <button
            onClick={handleGenerateReport}
            disabled={generateReportMutation.isPending}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {generateReportMutation.isPending ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Generating...
              </>
            ) : (
              <>
                <Play className="h-4 w-4 mr-2" />
                Generate Report
              </>
            )}
          </button>
        </div>
      </div>

      {/* Report Types Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {reportTypes?.map((type) => (
          <div key={type.value} className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow">
            <div className="flex items-start">
              <span className="text-3xl mr-3">
                {getReportTypeIcon(type.value as ReportType)}
              </span>
              <div className="flex-1">
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                  {type.name}
                </h3>
                <p className="text-sm text-gray-600 mb-4">
                  {type.description}
                </p>
                <button
                  onClick={() => {
                    setSelectedReportType(type.value as ReportType);
                    handleGenerateReport();
                  }}
                  className="inline-flex items-center px-3 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                >
                  <FileText className="h-4 w-4 mr-2" />
                  Generate
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Recent Reports */}
      <div className="bg-white rounded-lg border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-medium text-gray-900">Recent Reports</h3>
        </div>
        <div className="divide-y divide-gray-200">
          {/* This would be populated with actual report data */}
          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <FileText className="h-5 w-5 text-gray-400 mr-3" />
                <div>
                  <p className="text-sm font-medium text-gray-900">Active Linked Devices</p>
                  <p className="text-sm text-gray-500">Generated 2 hours ago</p>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <span className="inline-flex items-center px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                  <CheckCircle className="h-3 w-3 mr-1" />
                  Completed
                </span>
                <button className="text-blue-600 hover:text-blue-900">
                  <Download className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>

          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <FileText className="h-5 w-5 text-gray-400 mr-3" />
                <div>
                  <p className="text-sm font-medium text-gray-900">Unmatched SIMs</p>
                  <p className="text-sm text-gray-500">Generated 1 day ago</p>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <span className="inline-flex items-center px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                  <CheckCircle className="h-3 w-3 mr-1" />
                  Completed
                </span>
                <button className="text-blue-600 hover:text-blue-900">
                  <Download className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>

          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <FileText className="h-5 w-5 text-gray-400 mr-3" />
                <div>
                  <p className="text-sm font-medium text-gray-900">Inactive Devices</p>
                  <p className="text-sm text-gray-500">Failed 3 hours ago</p>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <span className="inline-flex items-center px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800">
                  <XCircle className="h-3 w-3 mr-1" />
                  Failed
                </span>
                <button className="text-gray-400 cursor-not-allowed">
                  <Download className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Help Section */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
        <div className="flex">
          <AlertTriangle className="h-5 w-5 text-blue-400 mr-3 mt-0.5" />
          <div>
            <h3 className="text-sm font-medium text-blue-800">About Reports</h3>
            <div className="mt-2 text-sm text-blue-700">
              <p>
                Reports are generated as CSV files and can be downloaded for external analysis. 
                Large reports may take several minutes to generate. You can monitor the status 
                of your reports in the Recent Reports section above.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}