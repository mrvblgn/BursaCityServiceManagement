import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Button,
  Chip,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import LocationCityIcon from '@mui/icons-material/LocationCity';
import LogoutIcon from '@mui/icons-material/Logout';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import { useAuth } from '../../auth/useAuth';
import { UserRole } from '../../types/auth.types';

interface AppHeaderProps {
  onToggleSidebar: () => void;
}

const roleLabels: Record<UserRole, string> = {
  Citizen: 'Vatandaş',
  Manager: 'Birim Yöneticisi',
  Employee: 'Saha Personeli',
  Admin: 'Sistem Yöneticisi',
};

export const AppHeader: React.FC<AppHeaderProps> = ({ onToggleSidebar }) => {
  const { user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        backgroundColor: '#FFFFFF',
        color: '#1E293B',
        borderBottom: '1px solid #E2E8F0',
        zIndex: (theme) => theme.zIndex.drawer + 1,
      }}
    >
      <Toolbar sx={{ justifyContent: 'space-between', minHeight: 64 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          {isMobile && (
            <IconButton
              color="inherit"
              aria-label="menüyü aç"
              edge="start"
              onClick={onToggleSidebar}
              sx={{ mr: 1 }}
            >
              <MenuIcon />
            </IconButton>
          )}

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box
              sx={{
                width: 38,
                height: 38,
                borderRadius: 1.5,
                backgroundColor: 'primary.main',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#FFFFFF',
              }}
            >
              <LocationCityIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography
                variant="subtitle1"
                sx={{
                  fontWeight: 700,
                  lineHeight: 1.2,
                  color: 'primary.dark',
                  letterSpacing: '-0.01em',
                }}
              >
                BURSA BÜYÜKŞEHİR BELEDİYESİ
              </Typography>
              <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 500 }}>
                Hizmet ve Talep Yönetim Sistemi (BCSMS)
              </Typography>
            </Box>
          </Box>
        </Box>

        {user && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ display: { xs: 'none', sm: 'flex' }, alignItems: 'center', gap: 1 }}>
              <AccountCircleIcon sx={{ color: '#64748B' }} />
              <Box sx={{ textAlign: 'right' }}>
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#1E293B' }}>
                  {user.firstName} {user.lastName}
                </Typography>
                <Chip
                  label={roleLabels[user.role] || user.role}
                  size="small"
                  color="primary"
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.7rem', fontWeight: 600 }}
                />
              </Box>
            </Box>

            <Button
              variant="outlined"
              color="inherit"
              size="small"
              startIcon={<LogoutIcon />}
              onClick={logout}
              sx={{
                borderColor: '#CBD5E1',
                color: '#475569',
                '&:hover': {
                  borderColor: '#94A3B8',
                  backgroundColor: '#F1F5F9',
                },
              }}
            >
              Çıkış
            </Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  );
};
