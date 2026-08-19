import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Alert,
  CircularProgress,
  DialogContentText,
} from '@mui/material';

interface NoteActionModalProps {
  open: boolean;
  title: string;
  description?: string;
  noteLabel?: string;
  confirmText: string;
  confirmColor?: 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success';
  isLoading?: boolean;
  error?: string | null;
  onConfirm: (note?: string) => void;
  onClose: () => void;
}

export const NoteActionModal: React.FC<NoteActionModalProps> = ({
  open,
  title,
  description,
  noteLabel = 'İşlem Notu / Açıklama (İsteğe bağlı)',
  confirmText,
  confirmColor = 'primary',
  isLoading = false,
  error = null,
  onConfirm,
  onClose,
}) => {
  const [note, setNote] = useState('');

  useEffect(() => {
    if (!open) {
      setNote('');
    }
  }, [open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onConfirm(note.trim() || undefined);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth aria-labelledby="note-action-dialog-title">
      <DialogTitle id="note-action-dialog-title" sx={{ fontWeight: 600 }}>
        {title}
      </DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent dividers>
          {description && <DialogContentText sx={{ mb: 2 }}>{description}</DialogContentText>}

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <TextField
            autoFocus
            fullWidth
            multiline
            rows={3}
            id="action-note"
            label={noteLabel}
            placeholder="İlgili karara veya işleme dair gerekçe/not yazabilirsiniz..."
            value={note}
            onChange={(e) => setNote(e.target.value)}
            disabled={isLoading}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={onClose} disabled={isLoading} color="inherit">
            İptal
          </Button>
          <Button
            type="submit"
            variant="contained"
            color={confirmColor}
            disabled={isLoading}
            startIcon={isLoading ? <CircularProgress size={18} color="inherit" /> : null}
          >
            {isLoading ? 'İşleniyor...' : confirmText}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
