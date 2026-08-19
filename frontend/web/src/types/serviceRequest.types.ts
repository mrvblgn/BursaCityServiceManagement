export type RequestStatus =
  | 'New'
  | 'Reviewing'
  | 'Assigned'
  | 'InProgress'
  | 'Resolved'
  | 'Closed'
  | 'Rejected'
  | 'Cancelled';

export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface LocationDto {
  latitude?: number | null;
  longitude?: number | null;
  addressText?: string | null;
}

export interface StatusHistoryEntryDto {
  id: string;
  oldStatus: RequestStatus;
  newStatus: RequestStatus;
  note?: string | null;
  changedByUserId: string;
  changedAt: string;
}

export interface CommentDto {
  id: string;
  content: string;
  createdByUserId: string;
  createdAt: string;
}

export interface AttachmentDto {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeInBytes: number;
  storagePath: string;
  uploadedByUserId: string;
  uploadedAt: string;
}

export interface ServiceRequestSummaryDto {
  id: string;
  title: string;
  categoryId: string;
  categoryName: string;
  status: RequestStatus;
  priority?: Priority | null;
  location?: LocationDto | null;
  createdAt: string;
}

export interface MunicipalServiceRequestSummaryDto {
  id: string;
  title: string;
  categoryId: string;
  categoryName: string;
  citizenId: string;
  citizenName: string;
  status: RequestStatus;
  priority?: Priority | null;
  assignedDepartmentId?: string | null;
  assignedDepartmentName?: string | null;
  assignedEmployeeId?: string | null;
  assignedEmployeeName?: string | null;
  location?: LocationDto | null;
  createdAt: string;
}

export interface EmployeeServiceRequestSummaryDto {
  id: string;
  title: string;
  categoryId: string;
  categoryName: string;
  status: RequestStatus;
  priority?: Priority | null;
  location?: LocationDto | null;
  createdAt: string;
}

export interface ServiceRequestDetailDto {
  id: string;
  title: string;
  description?: string | null;
  categoryId: string;
  categoryName: string;
  status: RequestStatus;
  priority?: Priority | null;
  location?: LocationDto | null;
  citizenId: string;
  assignedDepartmentId?: string | null;
  assignedEmployeeId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  statusHistory: StatusHistoryEntryDto[];
  comments: CommentDto[];
  attachments: AttachmentDto[];
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateServiceRequestInput {
  title: string;
  categoryId: string;
  description?: string;
  latitude?: number | null;
  longitude?: number | null;
  addressText?: string;
}

export interface AssignRequestInput {
  departmentId: string;
  employeeId: string;
  priority: Priority;
}

export interface WorkflowNoteInput {
  note?: string;
}
