import { apiClient } from './axios';
import { CategoryLookupDto, DepartmentLookupDto, EmployeeLookupDto } from '../types/reference.types';

export const referenceApi = {
  getCategories: async (): Promise<CategoryLookupDto[]> => {
    const response = await apiClient.get<CategoryLookupDto[]>('/api/categories');
    return response.data;
  },

  getDepartments: async (): Promise<DepartmentLookupDto[]> => {
    const response = await apiClient.get<DepartmentLookupDto[]>('/api/departments');
    return response.data;
  },

  getDepartmentEmployees: async (departmentId: string): Promise<EmployeeLookupDto[]> => {
    const response = await apiClient.get<EmployeeLookupDto[]>(`/api/departments/${departmentId}/employees`);
    return response.data;
  },
};
