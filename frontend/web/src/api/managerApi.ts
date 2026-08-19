import { apiClient } from './axios';
import {
  AssignRequestInput,
  MunicipalServiceRequestSummaryDto,
  PagedResult,
  Priority,
  RequestStatus,
  ServiceRequestDetailDto,
} from '../types/serviceRequest.types';

export interface ManagerRequestFilters {
  status?: RequestStatus | '';
  categoryId?: string | '';
  departmentId?: string | '';
  priority?: Priority | '';
  pageNumber?: number;
  pageSize?: number;
}

export const managerApi = {
  getMunicipalRequests: async (
    filters: ManagerRequestFilters = {}
  ): Promise<PagedResult<MunicipalServiceRequestSummaryDto>> => {
    const params = new URLSearchParams();
    if (filters.status) params.append('status', filters.status);
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.departmentId) params.append('departmentId', filters.departmentId);
    if (filters.priority) params.append('priority', filters.priority);
    params.append('pageNumber', (filters.pageNumber || 1).toString());
    params.append('pageSize', (filters.pageSize || 10).toString());

    const response = await apiClient.get<PagedResult<MunicipalServiceRequestSummaryDto>>(
      `/api/manager/service-requests?${params.toString()}`
    );
    return response.data;
  },

  getRequestById: async (id: string): Promise<ServiceRequestDetailDto> => {
    const response = await apiClient.get<ServiceRequestDetailDto>(`/api/manager/service-requests/${id}`);
    return response.data;
  },

  startReview: async (id: string): Promise<void> => {
    await apiClient.post(`/api/manager/service-requests/${id}/review`);
  },

  assignRequest: async (id: string, payload: AssignRequestInput): Promise<void> => {
    await apiClient.post(`/api/manager/service-requests/${id}/assign`, payload);
  },

  rejectRequest: async (id: string, note?: string): Promise<void> => {
    await apiClient.post(`/api/manager/service-requests/${id}/reject`, { note });
  },

  closeRequest: async (id: string, note?: string): Promise<void> => {
    await apiClient.post(`/api/manager/service-requests/${id}/close`, { note });
  },

  reopenRequest: async (id: string, note?: string): Promise<void> => {
    await apiClient.post(`/api/manager/service-requests/${id}/reopen`, { note });
  },
};
