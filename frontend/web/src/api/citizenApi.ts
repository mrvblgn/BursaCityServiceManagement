import { apiClient } from './axios';
import {
  CreateServiceRequestInput,
  PagedResult,
  RequestStatus,
  ServiceRequestDetailDto,
  ServiceRequestSummaryDto,
} from '../types/serviceRequest.types';

export const citizenApi = {
  createRequest: async (payload: CreateServiceRequestInput): Promise<{ id: string; trackingCode?: string }> => {
    const response = await apiClient.post<{ id: string; trackingCode?: string }>('/api/service-requests', payload);
    return response.data;
  },

  getMyRequests: async (
    status?: RequestStatus | '',
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<PagedResult<ServiceRequestSummaryDto>> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<PagedResult<ServiceRequestSummaryDto>>(
      `/api/service-requests?${params.toString()}`
    );
    return response.data;
  },

  getRequestById: async (id: string): Promise<ServiceRequestDetailDto> => {
    const response = await apiClient.get<ServiceRequestDetailDto>(`/api/service-requests/${id}`);
    return response.data;
  },
};
