import React, { useState, useEffect } from 'react';
import { useMutation } from '@tanstack/react-query';
import { TkButton, TkDatepicker, TkInput, TkSelect } from '@takeoff-ui/react';
import { dataProvider } from '../../dataProvider';
import { overlayStyle, modalStyle } from '../../styles';
import { LookupSelect } from '../../shared/LookupSelect';
import { useFlows } from '../../flows/FlowProvider';

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
};

interface SnagReportEditProps {
  record: SnagReportRecord | null;
  onClose: () => void;
  onSuccess: () => void;
}

export const SnagReportEdit: React.FC<SnagReportEditProps> = ({ record, onClose, onSuccess }) => {
  const [form, setForm] = useState<Partial<SnagReportRecord>>(record ?? {});
  const setField = (name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value }));
  const { triggerFlows } = useFlows();

  useEffect(() => {
    if (record) setForm({ ...record });
  }, [record]);

  const mutation = useMutation({
    mutationFn: (values: Partial<SnagReportRecord>) =>
      dataProvider.update('SnagReport', record!.id, values),
    onSuccess: (_data, values) => { triggerFlows('update', 'SnagReport', values as Record<string, unknown>); onSuccess(); onClose(); },
    onError: (err: Error) => { window.dispatchEvent(new CustomEvent('app-toast', { detail: { type: 'error', message: err.message } })); },
  });

  if (!record) return null;

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={modalStyle} onClick={(e) => e.stopPropagation()}>
        <div style={{ padding: '20px 28px 0', flexShrink: 0, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700 }}>Edit SnagReport</h2>
          <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: 20, cursor: 'pointer', color: '#666', padding: '4px 8px', borderRadius: 4 }} onMouseOver={(e) => (e.currentTarget.style.color = '#333')} onMouseOut={(e) => (e.currentTarget.style.color = '#666')}>✕</button>
        </div>
        <form onSubmit={(e) => { e.preventDefault(); mutation.mutate(form); }} style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
          <div style={{ flex: 1, overflowY: 'auto', padding: '20px 28px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div>
                  <TkInput mode="text" label="Report Number *" value={String(form.reportNumber ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('reportNumber', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Ata Chapter *" value={String(form.ataChapter ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('ataChapter', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Title *" value={String(form.title ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('title', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Description *" value={String(form.description ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('description', v))(e.detail)} />
                </div>
                <div>
                  <LookupSelect label="Severity *" value={String(form.severity ?? '')} onChange={(v) => setField('severity', v ? Number(v) : null)} searchable={false} options={[{ label: 'AOG', value: '0' }, { label: 'MEL', value: '1' }, { label: 'Routine', value: '2' }]} />
                </div>
                <div>
                  <TkDatepicker label="Detected At *" value={String(form.detectedAt ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('detectedAt', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Action Description" value={String(form.actionDescription ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('actionDescription', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Revision Note" value={String(form.revisionNote ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('revisionNote', v))(e.detail)} />
                </div>
                <div>
                  <LookupSelect label="Status *" value={String(form.status ?? '')} onChange={(v) => setField('status', v ? Number(v) : null)} searchable={false} options={[{ label: 'Open', value: '0' }, { label: 'InProgress', value: '1' }, { label: 'PendingCRS', value: '2' }, { label: 'Closed', value: '3' }]} />
                </div>
                <div>
                  <TkInput mode="number" label="Certifying Staff Id" value={String(form.certifyingStaffId ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('certifyingStaffId', Number(v)))(e.detail)} />
                </div>
                <div>
                  <LookupSelect label="Uçak *" resource="Aircraft" value={String(form.aircraftId ?? '')} onChange={(v) => setField('aircraftId', v)} displayField="registration" />
                </div>
                <div>
                  <LookupSelect label="Personel *" resource="Personnel" value={String(form.personnelId ?? '')} onChange={(v) => setField('personnelId', v)} displayField="firstName" />
                </div>
            </div>
          </div>
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, padding: '16px 28px', borderTop: '1px solid #e8e8e8', flexShrink: 0, background: '#fff' }}>
            <TkButton label="Cancel" variant="secondary" onTkClick={onClose} />
            <TkButton label={mutation.isPending ? 'Saving…' : 'Save Changes'} variant="primary" mode="submit" disabled={mutation.isPending} />
          </div>
        </form>
      </div>
    </div>
  );
};
