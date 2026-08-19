import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';

export const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <Box
      sx={{
        minHeight: '80vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 3,
      }}
    >
      <Paper
        elevation={0}
        sx={{
          p: { xs: 4, sm: 6 },
          textAlign: 'center',
          maxWidth: 500,
          borderRadius: 3,
          border: '1px solid #E2E8F0',
        }}
      >
        <ErrorOutlineIcon sx={{ fontSize: 64, color: '#94A3B8', mb: 2 }} />
        <Typography variant="h4" sx={{ fontWeight: 700, color: '#0F172A', mb: 1 }}>
          404
        </Typography>
        <Typography variant="h6" sx={{ fontWeight: 600, color: '#334155', mb: 1 }}>
          Sayfa Bulunamadı
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Ulaşmaya çalıştığınız sayfa kaldırılmış, adı değiştirilmiş veya geçici olarak kullanım dışı olabilir.
        </Typography>
        <Button variant="contained" color="primary" onClick={() => navigate('/')}>
          Ana Sayfaya Dön
        </Button>
      </Paper>
    </Box>
  );
};
