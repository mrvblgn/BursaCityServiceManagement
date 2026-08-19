import { createTheme } from '@mui/material/styles';

export const municipalTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#0D47A1', // Bursa municipal deep navy
      light: '#1E88E5',
      dark: '#002171',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#00695C', // Municipal emerald teal
      light: '#4DB6AC',
      dark: '#004D40',
      contrastText: '#FFFFFF',
    },
    background: {
      default: '#F5F7FA',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1E293B',
      secondary: '#64748B',
    },
    divider: '#E2E8F0',
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 700,
      color: '#0F172A',
    },
    h5: {
      fontWeight: 600,
      color: '#0F172A',
    },
    h6: {
      fontWeight: 600,
      color: '#1E293B',
    },
    subtitle1: {
      color: '#64748B',
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          boxShadow: 'none',
          '&:hover': {
            boxShadow: 'none',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          border: '1px solid #E2E8F0',
          boxShadow: '0px 1px 3px rgba(0, 0, 0, 0.05)',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
  },
});
