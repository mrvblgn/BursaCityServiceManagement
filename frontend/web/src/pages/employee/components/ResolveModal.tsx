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
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';

interface ResolveModalProps {
  open: boolean;
  isLoading?: boolean;
  error?: string | null;
  onConfirm: (note?: string) => void;
  onClose: () => void;
}

export const ResolveModal: React.FC<ResolveModalProps> = ({
  open,
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
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth aria-labelledby="resolve-modal-title">
      <DialogTitle id="resolve-modal-title" sx={{ fontWeight: 600 }}>
        Görevi Çözümlendi Olarak İşaretle
      </DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent dividers>
          <DialogContentText sx={{ mb: 2 }}>
            Saha çalışmasının tamamlandığını bildirmektesiniz. Yapılan işlem hakkında özet bir açıklama giriniz.
          </DialogContentText>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <TextField
            autoFocus
            fullWidth
            multiline
            rows={4}
            id="resolve-note"
            label="Çözüm Açıklaması / Notu"
            placeholder="Örn: 2 ton sıcak asfalt serimi yapıldı, rögar kapağı seviyeye getirildi ve yol trafiğe açıldı."
            value={note}
            onChange={(e) => setNote(e.target.value)}
            disabled={isLoading}
            helperText="Yönetici ve vatandaşın görebileceği özet çalışma bilgisi."
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={onClose} disabled={isLoading} color="inherit">
            İptal
          </Button>
          <Button
            type="submit"
            variant="contained"
            color="success"
            disabled={isLoading}
            startIcon={isLoading ? <CircularProgress size={18} color="inherit" /> : <CheckCircleOutlineIcon />}
          >
            {isLoading ? 'Kaydediliyor...' : 'Çözümü Kaydet'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
