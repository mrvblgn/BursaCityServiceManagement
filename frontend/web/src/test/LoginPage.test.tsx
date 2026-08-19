import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { LoginPage } from '../pages/auth/LoginPage';
import * as authApiModule from '../api/authApi';
import * as useAuthModule from '../auth/useAuth';

describe('LoginPage', () => {
  it('should render login form with email and password fields', () => {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    expect(screen.getByLabelText(/E-Posta Adresi/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Şifre/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Giriş Yap/i })).toBeInTheDocument();
  });

  it('should show error when submitting empty credentials', async () => {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
      login: vi.fn(),
      logout: vi.fn(),
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: /Giriş Yap/i }));

    expect(await screen.findByText(/Lütfen e-posta adresinizi ve şifrenizi giriniz/i)).toBeInTheDocument();
  });

  it('should call authApi.login and authContext.login on valid submit', async () => {
    const mockLogin = vi.fn();
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      token: null,
      user: null,
      isAuthenticated: false,
      isLoading: false,
      login: mockLogin,
      logout: vi.fn(),
    });

    const loginSpy = vi.spyOn(authApiModule.authApi, 'login').mockResolvedValue({
      accessToken: 'jwt-token-123',
      expiresAt: '2026-08-20T00:00:00Z',
      user: {
        id: '1',
        firstName: 'Ali',
        lastName: 'Vatandas',
        email: 'ali@bursa.bel.tr',
        role: 'Citizen',
      },
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/E-Posta Adresi/i), { target: { value: 'ali@bursa.bel.tr' } });
    fireEvent.change(screen.getByLabelText(/Şifre/i), { target: { value: 'Password123!' } });

    fireEvent.click(screen.getByRole('button', { name: /Giriş Yap/i }));

    await waitFor(() => {
      expect(loginSpy).toHaveBeenCalledWith({
        email: 'ali@bursa.bel.tr',
        password: 'Password123!',
      });
      expect(mockLogin).toHaveBeenCalled();
    });
  });
});
