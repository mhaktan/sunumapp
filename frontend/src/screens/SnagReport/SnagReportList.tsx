import React, { useState, useCallback } from 'react';
import { TkButton } from '@takeoff-ui/react';
import { useListQuery } from '../../shared/useListQuery';
import { useDeleteMutation } from '../../shared/useDeleteMutation';
import { DeleteConfirmDialog } from '../../shared/DeleteConfirmDialog';
import { ListPageLayout } from '../../shared/ListPageLayout';
import type { TableColumn } from '../../shared/ListPageLayout';
import { actionColumn } from '../../shared/ActionButtons';
import { SnagReportCreate } from './SnagReportCreate';
import { SnagReportEdit } from './SnagReportEdit';
import { useFlows } from '../../flows/FlowProvider';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

type SnagReportRecord = {
  id: string | number;
  reportNumber: string;
  ataChapter: string;
  title: string;
  description: string;
  severity: string;
  detectedAt: string;
  actionDescription?: string;
  revisionNote?: string;
  status: string;
  certifyingStaffId?: number;
  aircraftId: string;
  personnelId: string;
  [key: string]: unknown;
};

// ---------------------------------------------------------------------------
// Column definition — edit this array to add/remove/reorder columns
// ---------------------------------------------------------------------------
//
// Override examples:
//   • Hide a column:     remove its entry from COLUMNS
//   • Add a custom col:  { field: 'fullName', header: 'Full Name', html: (row) => `${row.firstName} ${row.lastName}` }
//   • Enable filtering:  add searchable: true  or  filterType: 'text' | 'checkbox' | 'radio' | 'datepicker'
//   • Custom cell render: html: (row) => `<span style="color:green">${row.status}</span>`
//
// Shared components (src/shared/) can be edited to change behavior globally:
//   • ListPageLayout  — table wrapper, pagination, header layout
//   • ActionButtons   — edit/delete button styles, labels, and behavior
//   • useListQuery    — data fetching, sorting, filtering logic
//   • useDeleteMutation / DeleteConfirmDialog — delete flow
//
// Action buttons override: edit src/shared/ActionButtons.ts DEFAULT_CONFIG
// or pass custom config: actionColumn('id', { hasEdit: true, config: { edit: { label: 'View', style: '...' } } })
//

const COLUMNS: TableColumn[] = [
  { field: 'id', header: 'ID', sortable: true },
  { field: 'reportNumber', header: 'Report Number', sortable: true, searchable: true, filterType: 'text' },
  { field: 'ataChapter', header: 'Ata Chapter', sortable: true, searchable: true, filterType: 'text' },
  { field: 'title', header: 'Title', sortable: true, searchable: true, filterType: 'text' },
  { field: 'description', header: 'Description', sortable: true, searchable: true, filterType: 'text' },
  { field: 'severity', header: 'Severity', sortable: true, filterType: 'radio', filterOptions: [{ label: 'AOG', value: '0' }, { label: 'MEL', value: '1' }, { label: 'Routine', value: '2' }], html: (row: Record<string, unknown>) => { const m: Record<string, string> = {'0': 'AOG', '1': 'MEL', '2': 'Routine'}; return m[String(row.severity ?? '')] ?? String(row.severity ?? '\u2014'); } },
  { field: 'detectedAt', header: 'Detected At', sortable: true, filterType: 'datepicker', html: (row: Record<string, unknown>) => row.detectedAt ? new Date(String(row.detectedAt)).toLocaleDateString() : '—' },
  { field: 'actionDescription', header: 'Action Description', sortable: true, searchable: true, filterType: 'text' },
  { field: 'revisionNote', header: 'Revision Note', sortable: true, searchable: true, filterType: 'text' },
  { field: 'status', header: 'Status', sortable: true, filterType: 'radio', filterOptions: [{ label: 'Open', value: '0' }, { label: 'InProgress', value: '1' }, { label: 'PendingCRS', value: '2' }, { label: 'Closed', value: '3' }], html: (row: Record<string, unknown>) => { const m: Record<string, string> = {'0': 'Open', '1': 'InProgress', '2': 'PendingCRS', '3': 'Closed'}; return m[String(row.status ?? '')] ?? String(row.status ?? '\u2014'); } },
  { field: 'certifyingStaffId', header: 'Certifying Staff Id', sortable: true },
  { field: 'aircraftId', header: 'Uçak', sortable: true },
  { field: 'personnelId', header: 'Personel', sortable: true },
];

// ---------------------------------------------------------------------------
// SnagReportList
// ---------------------------------------------------------------------------

export const SnagReportList: React.FC = () => {
  const list = useListQuery<SnagReportRecord>({ resource: 'SnagReport' });
  const [showCreate, setShowCreate] = useState(false);
  const [editRecord, setEditRecord] = useState<SnagReportRecord | null>(null);
  const [selectedRows, setSelectedRows] = useState<SnagReportRecord[]>([]);
  const { triggerFlows } = useFlows();
  const del = useDeleteMutation('SnagReport', (ids) => { ids.forEach(id => triggerFlows('delete', 'SnagReport', { id })); });


  const columns = [...COLUMNS, ...actionColumn('id', { hasEdit: true, hasDelete: true })];

  const handleCrudAction = useCallback((action: string, id: string) => {
    const row = list.records.find((r) => String(r.id) === id);
    if (!row) return;
    if (action === 'edit') setEditRecord(row);
    if (action === 'delete') del.requestSingleDelete(row.id);
  }, [list.records]);

  return (
    <>
      <ListPageLayout
        title="SnagReport"
        subtitle={Object.keys(list.displayParams).length > 0 ? (
          <div style={{ fontSize: 13, color: '#666', marginTop: 4 }}>
            {Object.entries(list.displayParams).map(([k, v]) => (
              <span key={k} style={{ marginRight: 12 }}>{k}: <strong>{v}</strong></span>
            ))}
          </div>
        ) : undefined}
        records={list.records}
        columns={columns}
        dataKey="id"
        total={list.total}
        loading={list.isLoading}
        page={list.page}
        perPage={list.perPage}
        onPageChange={list.setPage}
        onPerPageChange={list.setPerPage}
        onTableRequest={list.handleTableRequest}
        selectionMode="checkbox"
        selectedRows={selectedRows}
        onSelectionChange={(rows) => setSelectedRows(rows as SnagReportRecord[])}
        onCrudAction={handleCrudAction}
        headerActions={<>
          {selectedRows.length > 0 && (
            <TkButton label={`Delete (${selectedRows.length})`} variant="danger" onTkClick={() => del.requestDelete(selectedRows.map(r => r.id), `${selectedRows.length} record(s)`)} />
          )}

          <TkButton label="+ Create SnagReport" variant="primary" onTkClick={() => setShowCreate(true)} />
        </>}
      />

      {list.isError && (
        <div style={{ padding: '10px 14px', background: '#fff3f3', border: '1px solid #f5c6c6', borderRadius: 6, color: '#c62828', fontSize: 13, marginBottom: 12 }}>
          Failed to load data: {(list.error as Error).message}
        </div>
      )}

      <DeleteConfirmDialog
        visible={!!del.deleteTarget}
        label={del.deleteTarget?.label ?? ''}
        isPending={del.isPending}
        onConfirm={del.confirmDelete}
        onCancel={() => del.setDeleteTarget(null)}
      />
      <SnagReportCreate open={showCreate} onClose={() => setShowCreate(false)} onSuccess={list.invalidate} />
      <SnagReportEdit record={editRecord} onClose={() => setEditRecord(null)} onSuccess={list.invalidate} />

    </>
  );
};

