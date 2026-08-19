import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { ProtectedRoute } from '../auth/ProtectedRoute';
import { MainLayout } from '../components/layout/MainLayout';

// Auth Pages
import { LoginPage } from '../pages/auth/LoginPage';
import { RegisterPage } from '../pages/auth/RegisterPage';

// Citizen Pages
import { CitizenDashboardPage } from '../pages/citizen/CitizenDashboardPage';
import { CitizenRequestsPage } from '../pages/citizen/CitizenRequestsPage';
import { CreateRequestPage } from '../pages/citizen/CreateRequestPage';
import { CitizenDetailPage } from '../pages/citizen/CitizenDetailPage';

// Manager Pages
import { ManagerDashboardPage } from '../pages/manager/ManagerDashboardPage';
import { ManagerRequestsPage } from '../pages/manager/ManagerRequestsPage';
import { ManagerDetailPage } from '../pages/manager/ManagerDetailPage';

// Employee Pages
import { EmployeeDashboardPage } from '../pages/employee/EmployeeDashboardPage';
import { EmployeeRequestsPage } from '../pages/employee/EmployeeRequestsPage';
import { EmployeeDetailPage } from '../pages/employee/EmployeeDetailPage';

// 404
import { NotFoundPage } from '../pages/NotFoundPage';

// Root Redirect Component
const RootRedirect: React.FC = () => {
  const { isAuthenticated, user, isLoading } = useAuth();

  if (isLoading) return null;

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  if (user.role === 'Citizen') return <Navigate to="/citizen" replace />;
  if (user.role === 'Manager' || user.role === 'Admin') return <Navigate to="/manager" replace />;
  if (user.role === 'Employee') return <Navigate to="/employee" replace />;

  return <Navigate to="/login" replace />;
};

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Root redirect */}
      <Route path="/" element={<RootRedirect />} />

      {/* Public Auth Routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* Citizen Protected Routes */}
      <Route element={<ProtectedRoute allowedRoles={['Citizen']} />}>
        <Route element={<MainLayout />}>
          <Route path="/citizen" element={<CitizenDashboardPage />} />
          <Route path="/citizen/requests" element={<CitizenRequestsPage />} />
          <Route path="/citizen/requests/new" element={<CreateRequestPage />} />
          <Route path="/citizen/requests/:id" element={<CitizenDetailPage />} />
        </Route>
      </Route>

      {/* Manager Protected Routes */}
      <Route element={<ProtectedRoute allowedRoles={['Manager', 'Admin']} />}>
        <Route element={<MainLayout />}>
          <Route path="/manager" element={<ManagerDashboardPage />} />
          <Route path="/manager/requests" element={<ManagerRequestsPage />} />
          <Route path="/manager/requests/:id" element={<ManagerDetailPage />} />
        </Route>
      </Route>

      {/* Employee Protected Routes */}
      <Route element={<ProtectedRoute allowedRoles={['Employee']} />}>
        <Route element={<MainLayout />}>
          <Route path="/employee" element={<EmployeeDashboardPage />} />
          <Route path="/employee/requests" element={<EmployeeRequestsPage />} />
          <Route path="/employee/requests/:id" element={<EmployeeDetailPage />} />
        </Route>
      </Route>

      {/* 404 Catch-all */}
      <Route element={<MainLayout />}>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
};
