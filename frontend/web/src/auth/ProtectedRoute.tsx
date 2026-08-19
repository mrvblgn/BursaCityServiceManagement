import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';
import { UserRole } from '../types/auth.types';
import { Box, CircularProgress } from '@mui/material';

interface ProtectedRouteProps {
  allowedRoles?: UserRole[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { user, isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress color="primary" />
      </Box>
    );
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    // Redirect to user's assigned dashboard based on role
    if (user.role === 'Citizen') return <Navigate to="/citizen" replace />;
    if (user.role === 'Manager') return <Navigate to="/manager" replace />;
    if (user.role === 'Employee') return <Navigate to="/employee" replace />;
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
};
