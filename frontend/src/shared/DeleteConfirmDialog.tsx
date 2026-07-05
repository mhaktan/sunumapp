import React from 'react';
import { TkDialog, TkButton } from '@takeoff-ui/react';

interface DeleteConfirmDialogProps {
  visible: boolean;
  label: string;
  isPending: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export const DeleteConfirmDialog: React.FC<DeleteConfirmDialogProps> = ({
  visible, label, isPending, onConfirm, onCancel,
}) => (
  <TkDialog visible={visible} header="Confirm Delete" onTkClose={onCancel}>
    <div slot="content" style={{ padding: '16px 0', fontSize: 14, color: '#333' }}>
      Are you sure you want to delete <strong>{label}</strong>? This action cannot be undone.
    </div>
    <div slot="footer" style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', padding: '12px 16px', width: '100%', boxSizing: 'border-box' }}>
      <TkButton label="Cancel" variant="secondary" onTkClick={onCancel} />
      <TkButton
        label={isPending ? 'Deleting…' : 'Delete'}
        variant="danger"
        onTkClick={onConfirm}
        disabled={isPending}
      />
    </div>
  </TkDialog>
);
