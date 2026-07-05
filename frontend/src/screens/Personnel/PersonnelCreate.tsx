import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { TkButton, TkInput, TkSelect } from '@takeoff-ui/react';
import { dataProvider } from '../../dataProvider';
import { overlayStyle, modalStyle } from '../../styles';
import { LookupSelect } from '../../shared/LookupSelect';
import { useFlows } from '../../flows/FlowProvider';

type PersonnelRecord = {
  id: string | number;
  firstName: string;
  lastName: string;
  employeeNumber: string;
  role: string;
  licenseNumber?: string;
};

interface PersonnelCreateProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const PersonnelCreate: React.FC<PersonnelCreateProps> = ({ open, onClose, onSuccess }) => {
  const [form, setForm] = useState<Partial<PersonnelRecord>>({});
  const setField = (name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value }));
  const { triggerFlows } = useFlows();

  const mutation = useMutation({
    mutationFn: (values: Partial<PersonnelRecord>) => dataProvider.create('Personnel', values),
    onSuccess: (_data, values) => { triggerFlows('create', 'Personnel', values as Record<string, unknown>); onSuccess(); onClose(); setForm({}); },
    onError: (err: Error) => { window.dispatchEvent(new CustomEvent('app-toast', { detail: { type: 'error', message: err.message } })); },
  });

  if (!open) return null;

  return (
    <div style={overlayStyle} onClick={onClose}>
      <div style={modalStyle} onClick={(e) => e.stopPropagation()}>
        <div style={{ padding: '20px 28px 0', flexShrink: 0, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700 }}>Create Personnel</h2>
          <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: 20, cursor: 'pointer', color: '#666', padding: '4px 8px', borderRadius: 4 }} onMouseOver={(e) => (e.currentTarget.style.color = '#333')} onMouseOut={(e) => (e.currentTarget.style.color = '#666')}>✕</button>
        </div>
        <form onSubmit={(e) => { e.preventDefault(); mutation.mutate(form); }} style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
          <div style={{ flex: 1, overflowY: 'auto', padding: '20px 28px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div>
                  <TkInput mode="text" label="First Name *" value={String(form.firstName ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('firstName', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Last Name *" value={String(form.lastName ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('lastName', v))(e.detail)} />
                </div>
                <div>
                  <TkInput mode="text" label="Employee Number *" value={String(form.employeeNumber ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('employeeNumber', v))(e.detail)} />
                </div>
                <div>
                  <LookupSelect label="Role *" value={String(form.role ?? '')} onChange={(v) => setField('role', v ? Number(v) : null)} searchable={false} options={[{ label: 'LineMechanic', value: '0' }, { label: 'CertifyingStaff', value: '1' }]} />
                </div>
                <div>
                  <TkInput mode="text" label="License Number" value={String(form.licenseNumber ?? '')} onTkChange={(e: CustomEvent) => ((v) => setField('licenseNumber', v))(e.detail)} />
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
