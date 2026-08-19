import React, { createContext, useCallback, useEffect, useState } from 'react';
import { AuthUser, LoginResponse } from '../types/auth.types';
import { useQueryClient } from '@tanstack/react-query';

export interface AuthContextType {
  token: string | null;
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginResponse) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

const TOKEN_KEY = 'bcsms_access_token';
const USER_KEY = 'bcsms_user';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const queryClient = useQueryClient();
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Restore session from sessionStorage on app mount
  useEffect(() => {
    try {
      const storedToken = sessionStorage.getItem(TOKEN_KEY);
      const storedUser = sessionStorage.getItem(USER_KEY);
      if (storedToken && storedUser) {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      }
    } catch {
      sessionStorage.removeItem(TOKEN_KEY);
      sessionStorage.removeItem(USER_KEY);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
    queryClient.clear();
  }, [queryClient]);

  // Listen for global 401 unauthorized events
  useEffect(() => {
    const handleUnauthorized = () => {
      logout();
    };

    window.addEventListener('bcsms_unauthorized', handleUnauthorized);
    return () => window.removeEventListener('bcsms_unauthorized', handleUnauthorized);
  }, [logout]);

  const login = useCallback((data: LoginResponse) => {
    sessionStorage.setItem(TOKEN_KEY, data.accessToken);
    sessionStorage.setItem(USER_KEY, JSON.stringify(data.user));
    setToken(data.accessToken);
    setUser(data.user);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        isAuthenticated: !!token && !!user,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
