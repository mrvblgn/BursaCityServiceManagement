import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { ThemeProvider } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { municipalTheme } from '../theme/municipalTheme';
import { MainLayout } from '../components/layout/MainLayout';
import { CitizenDashboardPage } from '../pages/citizen/CitizenDashboardPage';
import { AuthContext } from '../auth/AuthContext';
import { AuthUser } from '../types/auth.types';

// Mock citizen user
const mockCitizenUser: AuthUser = {
  id: 'test-citizen-id',
  firstName: 'Ayşe',
  lastName: 'Demir',
  email: 'ayse@example.com',
  role: 'Citizen',
};

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: false } },
});

describe('MainLayout with Citizen Workflow', () => {
  it('renders fixed header, permanent sidebar navigation items, and citizen dashboard content', async () => {
    render(
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={municipalTheme}>
          <AuthContext.Provider
            value={{
              token: 'mock-token',
              user: mockCitizenUser,
              isAuthenticated: true,
              isLoading: false,
              login: vi.fn(),
              logout: vi.fn(),
            }}
          >
            <MemoryRouter initialEntries={['/citizen']}>
              <Routes>
                <Route element={<MainLayout />}>
                  <Route path="/citizen" element={<CitizenDashboardPage />} />
                </Route>
              </Routes>
            </MemoryRouter>
          </AuthContext.Provider>
        </ThemeProvider>
      </QueryClientProvider>
    );

    // 1. Verify Header content
    expect(screen.getByText(/BURSA BÜYÜKŞEHİR BELEDİYESİ/i)).toBeInTheDocument();
    expect(screen.getByText('Ayşe Demir')).toBeInTheDocument();
    expect(screen.getByText('Vatandaş')).toBeInTheDocument();

    // 2. Verify Sidebar Navigation items for Citizen
    expect(screen.getByRole('button', { name: /^Ana Sayfa$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^Başvurularım$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^Yeni Başvuru$/i })).toBeInTheDocument();

    // 3. Verify Dashboard Main Content
    expect(screen.getByText(/Hoş Geldiniz, Ayşe Demir/i)).toBeInTheDocument();
    expect(screen.getByText(/Toplam Başvurularım/i)).toBeInTheDocument();
    expect(screen.getByText(/Son Başvurular \(Bu Sayfa\)/i)).toBeInTheDocument();
    expect(screen.getByText(/Son Başvurularım/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^Yeni Başvuru Yap$/i })).toBeInTheDocument();
  });
});
