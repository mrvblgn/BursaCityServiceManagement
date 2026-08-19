import { apiClient } from './axios';
import { LoginResponse, RegisterResponse } from '../types/auth.types';

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  password: string;
}

export const authApi = {
  login: async (payload: LoginPayload): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/api/auth/login', payload);
    return response.data;
  },

  register: async (payload: RegisterPayload): Promise<RegisterResponse> => {
    const response = await apiClient.post<RegisterResponse>('/api/auth/register', payload);
    return response.data;
  },
};
