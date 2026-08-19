# RESTful API Overview

The **Bursa City Service Management System (BCSMS)** backend API is built with ASP.NET Core 8. All endpoints enforce standard HTTP methods, JWT Bearer authentication, and role-based authorization. Detailed request/response schemas are available via OpenAPI / Swagger UI at `/swagger`.

---

## 1. Authentication Endpoints (`/api/auth`)

| Method | Route | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Anonymous | Register a new citizen account. Returns user profile (201 Created). |
| `POST` | `/api/auth/login` | Anonymous | Authenticate with email & password. Returns signed JWT and user profile (200 OK). |

---

## 2. Citizen Endpoints (`/api/service-requests`)

*Requires `Authorization: Bearer <token>` with `Citizen` role.*

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/service-requests` | Create a new municipal service request. Returns tracking code & details (201 Created). |
| `GET` | `/api/service-requests/my` | List paginated service requests created by the authenticated citizen (supports status filter). |
| `GET` | `/api/service-requests/{id}` | Get full details, location, and status history timeline for a citizen request. |

---

## 3. Manager Endpoints (`/api/manager/service-requests`)

*Requires `Authorization: Bearer <token>` with `Manager` or `Admin` role.*

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/manager/service-requests` | List municipal requests with multi-filtering (`status`, `categoryId`, `departmentId`, `priority`, `pageNumber`, `pageSize`). |
| `GET` | `/api/manager/service-requests/{id}` | Retrieve comprehensive request details, citizen info, and workflow history. |
| `POST` | `/api/manager/service-requests/{id}/review` | Start review on a `New` request ($\rightarrow$ `Reviewing`). |
| `POST` | `/api/manager/service-requests/{id}/assign` | Assign request to a department and employee with priority level ($\rightarrow$ `Assigned`). |
| `POST` | `/api/manager/service-requests/{id}/reject` | Reject request with a mandatory rejection note ($\rightarrow$ `Rejected`). |
| `POST` | `/api/manager/service-requests/{id}/close` | Officially close a `Resolved` request with a closure note ($\rightarrow$ `Closed`). |
| `POST` | `/api/manager/service-requests/{id}/reopen` | Reopen a `Resolved` request with a reopen note ($\rightarrow$ `InProgress`). |

---

## 4. Field Employee Endpoints (`/api/employee/service-requests`)

*Requires `Authorization: Bearer <token>` with `Employee` role.*

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/employee/service-requests` | List tasks assigned specifically to the authenticated employee. |
| `GET` | `/api/employee/service-requests/{id}` | Retrieve assigned task details and location info. |
| `POST` | `/api/employee/service-requests/{id}/start` | Begin work on an assigned request ($\rightarrow$ `InProgress`). |
| `POST` | `/api/employee/service-requests/{id}/resolve` | Mark task as resolved with a detailed resolution note ($\rightarrow$ `Resolved`). |

---

## 5. Reference Data Lookup Endpoints (`/api`)

| Method | Route | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/categories` | `[Authorize]` | Active service request categories for dropdown selection (`Id`, `Name`, `Description`). |
| `GET` | `/api/departments` | `Manager`, `Admin` | Active municipal departments (`Id`, `Name`). |
| `GET` | `/api/departments/{id}/employees` | `Manager`, `Admin` | Active staff belonging to the specified department (`Id`, `FullName`, `Email`). |

---

## 6. Operational Endpoints

| Method | Route | Authorization | Description |
|---|---|---|---|
| `GET` | `/health` | Anonymous | Built-in ASP.NET Core liveness health check (`Healthy`). |
| `GET` | `/swagger` | Anonymous (Dev) | Interactive Swagger UI API explorer. |
