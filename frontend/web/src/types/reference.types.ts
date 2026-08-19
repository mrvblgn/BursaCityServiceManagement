export interface CategoryLookupDto {
  id: string;
  name: string;
  description?: string | null;
}

export interface DepartmentLookupDto {
  id: string;
  name: string;
}

export interface EmployeeLookupDto {
  id: string;
  fullName: string;
  email: string;
}
