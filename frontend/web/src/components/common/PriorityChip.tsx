import React from 'react';
import { Chip, ChipProps } from '@mui/material';
import { Priority } from '../../types/serviceRequest.types';

interface PriorityChipProps {
  priority?: Priority | null;
  size?: ChipProps['size'];
}

export const priorityConfig: Record<
  Priority,
  { label: string; color: ChipProps['color']; variant: ChipProps['variant'] }
> = {
  Low: { label: 'Düşük', color: 'success', variant: 'outlined' },
  Medium: { label: 'Orta', color: 'warning', variant: 'filled' },
  High: { label: 'Yüksek', color: 'error', variant: 'filled' },
  Critical: { label: 'Kritik', color: 'error', variant: 'filled' },
};

export const PriorityChip: React.FC<PriorityChipProps> = ({ priority, size = 'small' }) => {
  if (!priority) {
    return <Chip label="Belirtilmemiş" variant="outlined" size={size} sx={{ color: 'text.secondary' }} />;
  }

  const config = priorityConfig[priority] || { label: priority, color: 'default', variant: 'outlined' };

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
