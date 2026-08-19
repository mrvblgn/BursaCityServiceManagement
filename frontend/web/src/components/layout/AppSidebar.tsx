import React from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Box,
  Divider,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AssignmentIcon from '@mui/icons-material/Assignment';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import FormatListBulletedIcon from '@mui/icons-material/FormatListBulleted';
import EngineeringIcon from '@mui/icons-material/Engineering';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';

interface AppSidebarProps {
  open: boolean;
  onClose: () => void;
}

const DRAWER_WIDTH = 240;

export const AppSidebar: React.FC<AppSidebarProps> = ({ open, onClose }) => {
  const { user } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const getNavItems = () => {
    if (!user) return [];

    if (user.role === 'Citizen') {
      return [
        { label: 'Ana Sayfa', path: '/citizen', icon: <DashboardIcon /> },
        { label: 'Başvurularım', path: '/citizen/requests', icon: <AssignmentIcon /> },
        { label: 'Yeni Başvuru', path: '/citizen/requests/new', icon: <AddCircleOutlineIcon /> },
      ];
    }

    if (user.role === 'Manager') {
      return [
        { label: 'Ana Sayfa', path: '/manager', icon: <DashboardIcon /> },
        { label: 'Başvurular', path: '/manager/requests', icon: <FormatListBulletedIcon /> },
      ];
    }

    if (user.role === 'Employee') {
      return [
        { label: 'Ana Sayfa', path: '/employee', icon: <DashboardIcon /> },
        { label: 'Görevlerim', path: '/employee/requests', icon: <EngineeringIcon /> },
      ];
    }

    return [];
  };

  const navItems = getNavItems();

  const handleNavClick = (path: string) => {
    navigate(path);
    if (isMobile) {
      onClose();
    }
  };

  const drawerContent = (
    <Box sx={{ overflow: 'auto', py: 2 }}>
      <List sx={{ px: 1.5 }}>
        {navItems.map((item) => {
          const isSelected = location.pathname === item.path;
          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                selected={isSelected}
                onClick={() => handleNavClick(item.path)}
                sx={{
                  borderRadius: 2,
                  py: 1,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.main',
                    color: '#FFFFFF',
                    '&:hover': {
                      backgroundColor: 'primary.dark',
                    },
                    '& .MuiListItemIcon-root': {
                      color: '#FFFFFF',
                    },
                  },
                  '&:hover': {
                    backgroundColor: '#F1F5F9',
                  },
                }}
              >
                <ListItemIcon
                  sx={{
                    minWidth: 40,
                    color: isSelected ? '#FFFFFF' : '#64748B',
                  }}
                >
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{
                    fontSize: '0.9rem',
                    fontWeight: isSelected ? 600 : 500,
                  }}
                />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
      <Divider sx={{ my: 2 }} />
    </Box>
  );

  if (isMobile) {
    return (
      <Drawer
        variant="temporary"
        open={open}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            backgroundColor: '#FFFFFF',
            borderRight: '1px solid #E2E8F0',
          },
        }}
      >
        <Toolbar />
        {drawerContent}
      </Drawer>
    );
  }

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
          backgroundColor: '#FFFFFF',
          borderRight: '1px solid #E2E8F0',
          top: 64,
          height: 'calc(100% - 64px)',
        },
      }}
    >
      {drawerContent}
    </Drawer>
  );
};
