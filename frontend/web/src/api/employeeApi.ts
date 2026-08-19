import { apiClient } from './axios';
import {
  EmployeeServiceRequestSummaryDto,
  PagedResult,
  RequestStatus,
  ServiceRequestDetailDto,
} from '../types/serviceRequest.types';

export const employeeApi = {
  getMyAssignedRequests: async (
    status?: RequestStatus | '',
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<PagedResult<EmployeeServiceRequestSummaryDto>> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<PagedResult<EmployeeServiceRequestSummaryDto>>(
      `/api/employee/service-requests?${params.toString()}`
    );
    return response.data;
  },

  getAssignedRequestById: async (id: string): Promise<ServiceRequestDetailDto> => {
    const response = await apiClient.get<ServiceRequestDetailDto>(`/api/employee/service-requests/${id}`);
    return response.data;
  },

  startWork: async (id: string): Promise<void> => {
    await apiClient.post(`/api/employee/service-requests/${id}/start`);
  },

  resolveRequest: async (id: string, note?: string): Promise<void> => {
    await apiClient.post(`/api/employee/service-requests/${id}/resolve`, { note });
  },
};
