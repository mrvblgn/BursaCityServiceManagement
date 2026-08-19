import React from 'react';
import { Chip, ChipProps } from '@mui/material';
import { RequestStatus } from '../../types/serviceRequest.types';

interface StatusChipProps {
  status: RequestStatus;
  size?: ChipProps['size'];
}

export const statusConfig: Record<
  RequestStatus,
  { label: string; color: ChipProps['color']; variant: ChipProps['variant'] }
> = {
  New: { label: 'Yeni', color: 'info', variant: 'outlined' },
  Reviewing: { label: 'İnceleniyor', color: 'warning', variant: 'filled' },
  Assigned: { label: 'Atandı', color: 'primary', variant: 'filled' },
  InProgress: { label: 'İşlemde', color: 'secondary', variant: 'filled' },
  Resolved: { label: 'Çözüldü', color: 'success', variant: 'filled' },
  Closed: { label: 'Kapatıldı', color: 'default', variant: 'filled' },
  Rejected: { label: 'Reddedildi', color: 'error', variant: 'filled' },
  Cancelled: { label: 'İptal Edildi', color: 'default', variant: 'outlined' },
};

export const StatusChip: React.FC<StatusChipProps> = ({ status, size = 'small' }) => {
  const config = statusConfig[status] || { label: status, color: 'default', variant: 'outlined' };

  return (
    <Chip
      label={config.label}
      color={config.color}
      variant={config.variant}
      size={size}
      sx={{ fontWeight: 600, fontSize: size === 'small' ? '0.75rem' : '0.85rem' }}
    />
  );
};
