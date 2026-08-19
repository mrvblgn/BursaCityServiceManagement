import axios from 'axios';

const baseURL = import.meta.env.VITE_API_BASE_URL || '';

export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: attach bearer token from sessionStorage
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('bcsms_access_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: handle 401 Unauthorized globally
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      sessionStorage.removeItem('bcsms_access_token');
      sessionStorage.removeItem('bcsms_user');
      // Dispatch custom event so AuthContext and QueryClient can synchronize
      window.dispatchEvent(new CustomEvent('bcsms_unauthorized'));
    }
    return Promise.reject(error);
  }
);
