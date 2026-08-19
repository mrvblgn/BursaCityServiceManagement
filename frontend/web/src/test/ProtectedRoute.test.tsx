import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from '../auth/ProtectedRoute';
import * as useAuthModule from '../auth/useAuth';

describe('ProtectedRoute', () => {
  it('should redirect unauthenticated user to /login', () => {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={['/citizen']}>
        <Routes>
          <Route path="/login" element={<div>Giriş Sayfası</div>} />
          <Route element={<ProtectedRoute allowedRoles={['Citizen']} />}>
            <Route path="/citizen" element={<div>Vatandaş Paneli</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Giriş Sayfası')).toBeInTheDocument();
    expect(screen.queryByText('Vatandaş Paneli')).not.toBeInTheDocument();
  });

  it('should redirect citizen trying to access /manager to /citizen', () => {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: 'valid-token',
      user: {
        id: '1',
        firstName: 'Ali',
        lastName: 'Vatandas',
        email: 'ali@bursa.bel.tr',
        role: 'Citizen',
      },
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={['/manager']}>
        <Routes>
          <Route path="/citizen" element={<div>Vatandaş Paneli</div>} />
          <Route element={<ProtectedRoute allowedRoles={['Manager']} />}>
            <Route path="/manager" element={<div>Yönetici Paneli</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Vatandaş Paneli')).toBeInTheDocument();
    expect(screen.queryByText('Yönetici Paneli')).not.toBeInTheDocument();
  });

  it('should allow access when user has permitted role', () => {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: 'valid-token',
      user: {
        id: '2',
        firstName: 'Kemal',
        lastName: 'Yonetici',
        email: 'manager@bursa.bel.tr',
        role: 'Manager',
      },
      isAuthenticated: true,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter initialEntries={['/manager']}>
        <Routes>
          <Route element={<ProtectedRoute allowedRoles={['Manager']} />}>
            <Route path="/manager" element={<div>Yönetici Paneli</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Yönetici Paneli')).toBeInTheDocument();
  });
});
