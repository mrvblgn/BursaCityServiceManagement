import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { RegisterPage } from '../pages/auth/RegisterPage';
import * as authApiModule from '../api/authApi';

describe('RegisterPage', () => {
  it('should render registration fields', () => {
    const { container } = render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    expect(container.querySelector('#firstName')).toBeInTheDocument();
    expect(container.querySelector('#lastName')).toBeInTheDocument();
    expect(container.querySelector('#email')).toBeInTheDocument();
    expect(container.querySelector('#password')).toBeInTheDocument();
    expect(container.querySelector('#confirmPassword')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Kayıt Ol/i })).toBeInTheDocument();
  });

  it('should validate short password', async () => {
    const { container } = render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    fireEvent.change(container.querySelector('#firstName')!, { target: { value: 'Ali' } });
    fireEvent.change(container.querySelector('#lastName')!, { target: { value: 'Veli' } });
    fireEvent.change(container.querySelector('#email')!, { target: { value: 'ali@test.com' } });
    fireEvent.change(container.querySelector('#password')!, { target: { value: '123' } });
    fireEvent.change(container.querySelector('#confirmPassword')!, { target: { value: '123' } });

    fireEvent.click(screen.getByRole('button', { name: /Kayıt Ol/i }));

    expect(await screen.findByText(/Şifre en az 8 karakter uzunluğunda olmalıdır/i)).toBeInTheDocument();
  });

  it('should call authApi.register on valid registration submit', async () => {
    const registerSpy = vi.spyOn(authApiModule.authApi, 'register').mockResolvedValue({
      id: '1',
      firstName: 'Ali',
      lastName: 'Veli',
      email: 'ali@test.com',
      role: 'Citizen',
      createdAt: '2026-08-20T00:00:00Z',
    });

    const { container } = render(
      <MemoryRouter>
        <RegisterPage />
      </MemoryRouter>
    );

    fireEvent.change(container.querySelector('#firstName')!, { target: { value: 'Ali' } });
    fireEvent.change(container.querySelector('#lastName')!, { target: { value: 'Veli' } });
    fireEvent.change(container.querySelector('#email')!, { target: { value: 'ali@test.com' } });
    fireEvent.change(container.querySelector('#password')!, { target: { value: 'Password123!' } });
    fireEvent.change(container.querySelector('#confirmPassword')!, { target: { value: 'Password123!' } });

    fireEvent.click(screen.getByRole('button', { name: /Kayıt Ol/i }));

    await waitFor(() => {
      expect(registerSpy).toHaveBeenCalledWith({
        firstName: 'Ali',
        lastName: 'Veli',
        email: 'ali@test.com',
        phoneNumber: undefined,
        password: 'Password123!',
      });
    });
  });
});
