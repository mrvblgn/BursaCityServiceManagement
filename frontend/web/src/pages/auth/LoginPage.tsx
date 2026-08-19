import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Link,
  CircularProgress,
  Alert,
  Paper,
} from '@mui/material';
import LocationCityIcon from '@mui/icons-material/LocationCity';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { authApi } from '../../api/authApi';
import { getErrorMessage } from '../../utils/errorUtils';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !password) {
      setError('Lütfen e-posta adresinizi ve şifrenizi giriniz.');
      return;
    }

    setError(null);
    setIsLoading(true);

    try {
      const response = await authApi.login({ email: email.trim(), password });
      login(response);

      // Redirect according to role
      if (response.user.role === 'Citizen') {
        navigate('/citizen', { replace: true });
      } else if (response.user.role === 'Manager') {
        navigate('/manager', { replace: true });
      } else if (response.user.role === 'Employee') {
        navigate('/employee', { replace: true });
      } else {
        navigate('/', { replace: true });
      }
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#F1F5F9',
        p: 2,
      }}
    >
      <Card sx={{ maxWidth: 440, width: '100%', borderRadius: 3, boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }}>
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Paper
              elevation={0}
              sx={{
                width: 52,
                height: 52,
                borderRadius: 2,
                backgroundColor: 'primary.main',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#FFFFFF',
                mb: 1.5,
              }}
            >
              <LocationCityIcon fontSize="large" />
            </Paper>
            <Typography variant="h6" align="center" sx={{ fontWeight: 700, color: 'primary.dark' }}>
              BURSA BÜYÜKŞEHİR BELEDİYESİ
            </Typography>
            <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 0.5 }}>
              Hizmet ve Talep Yönetim Sistemi Girişi
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2.5 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="E-Posta Adresi"
              name="email"
              autoComplete="email"
              autoFocus
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isLoading}
              size="medium"
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Şifre"
              type="password"
              id="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={isLoading}
              size="medium"
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading}
              sx={{ mt: 3, mb: 2, py: 1.2, fontWeight: 600 }}
            >
              {isLoading ? <CircularProgress size={24} color="inherit" /> : 'Giriş Yap'}
            </Button>

            <Box sx={{ textAlign: 'center', mt: 2 }}>
              <Typography variant="body2" color="text.secondary">
                Hesabınız yok mu?{' '}
                <Link component={RouterLink} to="/register" variant="body2" sx={{ fontWeight: 600 }}>
                  Vatandaş Kaydı Oluştur
                </Link>
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};
