import React from 'react';
import { Alert, AlertTitle, Box, Button } from '@mui/material';
import { getErrorMessage } from '../../utils/errorUtils';

interface ErrorAlertProps {
  error: unknown;
  title?: string;
  onRetry?: () => void;
}

export const ErrorAlert: React.FC<ErrorAlertProps> = ({ error, title = 'Hata Oluştu', onRetry }) => {
  if (!error) return null;

  const message = getErrorMessage(error);

  return (
    <Box sx={{ my: 2 }}>
      <Alert
        severity="error"
        action={
          onRetry ? (
            <Button color="inherit" size="small" onClick={onRetry}>
              Tekrar Dene
            </Button>
          ) : undefined
        }
      >
        <AlertTitle sx={{ fontWeight: 600 }}>{title}</AlertTitle>
        {message}
      </Alert>
    </Box>
  );
};
