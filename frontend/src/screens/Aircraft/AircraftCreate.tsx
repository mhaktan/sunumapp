import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { TkButton, TkInput, TkSelect } from '@takeoff-ui/react';
import { dataProvider } from '../../dataProvider';
import { overlayStyle, modalStyle } from '../../styles';
import { LookupSelect } from '../../shared/LookupSelect';
import { useFlows } from '../../flows/FlowProvider';

type AircraftRecord = {
  id: string | number;
  registration: string;
  aircraftType: string;
  model: string;
  status: string;
};

interface AircraftCreateProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const AircraftCreate: React.FC<AircraftCreateProps> = ({ open, onClose, onSuccess }) => {
  const [form, setForm] = useState<Partial<AircraftRecord>>({});
  const setField = (name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value }));
  const { triggerFlows } = useFlows();

  const mutation = useMutation({
    mutationFn: (values: Partial<AircraftRecord>) => dataProvider.create('Aircraft', values),
    onSuccess: (_data, values) => { triggerFlows('create', 'Aircraft', values as Record<string, unknown>); onSuccess(); onClose(); setForm({}); },
    onError: (err: Error) => { window.dispatchEvent(new CustomEvent('app-toast', { detail: { type: 'error', message: err.message } })); },
  });

  if (!open) return null;

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={modalStyle} onClick={(e) => e.stopPropagation()}>
        <div style={{ padding: '20px 28px 0', flexShrink: 0, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700 }}>Create Aircraft</h2>
          <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: 20, cursor: 'pointer', color: '#666', padding: '4px 8px', borderRadius: 4 }} onMouseOver={(e) => (e.currentTarget.style.color = '#333')} onMouseOut={(e) => (e.currentTarget.style.color = '#666')}>✕</button>
        </div>
        <form onSubmit={(e) => { e.preventDefault(); mutation.mutate(form); }} style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
          <div style={{ flex: 1, overflowY: 'auto', padding: '20px 28px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div>
                  <TkInput mode="text" label="Registration *" value={String(form.registration ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('registration', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Aircraft Type *" value={String(form.aircraftType ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('aircraftType', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Model *" value={String(form.model ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('model', v))(e.detail)} />
                </div>
                <div>
                  <LookupSelect label="Status *" value={String(form.status ?? '')} onChange={(v) => setField('status', v ? Number(v) : null)} searchable={false} options={[{ label: 'InService', value: '0' }, { label: 'AOG', value: '1' }, { label: 'Maintenance', value: '2' }]} />
                </div>
            </div>
          </div>
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, padding: '16px 28px', borderTop: '1px solid #e8e8e8', flexShrink: 0, background: '#fff' }}>
            <TkButton label="Cancel" variant="secondary" onTkClick={onClose} />
            <TkButton label={mutation.isPending ? 'Saving…' : 'Create'} variant="primary" mode="submit" disabled={mutation.isPending} />
          </div>
        </form>
      </div>
    </div>
  );
};
