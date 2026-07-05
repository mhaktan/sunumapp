// ---------------------------------------------------------------------------
// Enum definitions — auto-generated from ER model
// Maps integer values to display labels for enum fields
// ---------------------------------------------------------------------------

// Aircraft.status
export const AircraftStatusMap: Record<string, string> = {
  '0': 'InService',
  '1': 'AOG',
  '2': 'Maintenance'
};
export const AircraftStatusOptions = [
  { label: 'InService', value: '0' },
  { label: 'AOG', value: '1' },
  { label: 'Maintenance', value: '2' }
];

// Personnel.role
export const PersonnelRoleMap: Record<string, string> = {
  '0': 'LineMechanic',
  '1': 'CertifyingStaff'
};
export const PersonnelRoleOptions = [
  { label: 'LineMechanic', value: '0' },
  { label: 'CertifyingStaff', value: '1' }
];

// SnagReport.severity
export const SnagReportSeverityMap: Record<string, string> = {
  '0': 'AOG',
  '1': 'MEL',
  '2': 'Routine'
};
export const SnagReportSeverityOptions = [
  { label: 'AOG', value: '0' },
  { label: 'MEL', value: '1' },
  { label: 'Routine', value: '2' }
];

// SnagReport.status
export const SnagReportStatusMap: Record<string, string> = {
  '0': 'Open',
  '1': 'InProgress',
  '2': 'PendingCRS',
  '3': 'Closed'
};
export const SnagReportStatusOptions = [
  { label: 'Open', value: '0' },
  { label: 'InProgress', value: '1' },
  { label: 'PendingCRS', value: '2' },
  { label: 'Closed', value: '3' }
];
