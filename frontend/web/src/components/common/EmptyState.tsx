import React from 'react';
import { Box, Typography, Button } from '@mui/material';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';

interface EmptyStateProps {
  title?: string;
  description?: string;
  actionText?: string;
  onAction?: () => void;
  icon?: React.ReactNode;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title = 'Kayıt Bulunamadı',
  description = 'Görüntülenecek herhangi bir başvuru veya kayıt bulunmamaktadır.',
  actionText,
  onAction,
  icon,
}) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 8,
        px: 3,
        textAlign: 'center',
        backgroundColor: '#FFFFFF',
        borderRadius: 2,
        border: '1px dashed #CBD5E1',
      }}
    >
      <Box sx={{ color: 'text.secondary', mb: 2 }}>
        {icon || <InboxOutlinedIcon sx={{ fontSize: 48, color: '#94A3B8' }} />}
      </Box>
      <Typography variant="h6" sx={{ fontWeight: 600, color: '#334155', mb: 1 }}>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 400, mb: actionText ? 3 : 0 }}>
        {description}
      </Typography>
      {actionText && onAction && (
        <Button variant="contained" color="primary" onClick={onAction} sx={{ mt: 2 }}>
          {actionText}
        </Button>
      )}
    </Box>
  );
};
