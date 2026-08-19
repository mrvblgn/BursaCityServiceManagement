# Submission & Evaluator Verification Checklist

This checklist documents the repository completeness, automated verification results, and manual evaluation instructions for the **Bursa City Service Management System (BCSMS)** case study submission.

---

## 1. Submission Verification Checklist

### Repository
- [x] Clean Git working tree with all required source code committed
- [x] Zero real secrets or private credentials tracked
- [x] Professional root [`README.md`](../README.md) present
- [x] Safe [`.env.example`](../.env.example) template present
- [x] `.gitignore` and `.dockerignore` properly configured

### Backend (.NET 8 & Clean Architecture)
- [x] Solution restores and compiles cleanly (`dotnet build` $\rightarrow$ 0 Errors, 0 Warnings)
- [x] Automated test suite passes 100% (`dotnet test` $\rightarrow$ **76/76 Tests Passed**)
  - `BCSMS.UnitTests`: 48 tests
  - `BCSMS.IntegrationTests`: 28 tests
- [x] EF Core 8 initial migrations and model snapshot included
- [x] Interactive OpenAPI / Swagger UI documentation configured

### Frontend (React 18 & TypeScript)
- [x] TypeScript typecheck passes cleanly (`npm run typecheck` $\rightarrow$ 0 Errors)
- [x] Automated test suite passes 100% (`npm test` $\rightarrow$ **28/28 Tests Passed**)
- [x] Production build bundle compiles successfully (`npm run build`)
- [x] Responsive layout with fixed header, collapsible sidebar, and role-based guards

### Infrastructure & Containerization
- [x] Docker Compose specification validated (`docker compose config`)
- [x] PostgreSQL 16 Alpine service configured with container healthcheck (`pg_isready`)
- [x] Named volume (`bcsms_postgres_data`) configured for database persistence
- [x] Nginx reverse proxy configured for SPA routing (`try_files`) and `/api/` proxying
- [x] Built-in `/health` check operational and responding `Healthy`

### Technical Documentation
- [x] Architecture specifications and Mermaid diagram ([`docs/architecture/architecture.md`](./architecture/architecture.md))
- [x] Database schema, table specs, and Mermaid ER diagram ([`docs/database/er-diagram.md`](./database/er-diagram.md))
- [x] Municipal request state machine and transition rules ([`docs/business/request-workflow.md`](./business/request-workflow.md))
- [x] RESTful API route catalog and authorization matrix ([`docs/api/api-overview.md`](./api/api-overview.md))

---

## 2. Manual Evaluator Verification Workflow

Follow these steps to reproduce the complete end-to-end municipal lifecycle on any standard Docker-capable environment:

### Step 1: Clone and Configure
```bash
git clone <repository_url>
cd BursaCityServiceManagement
cp .env.example .env
```

### Step 2: Launch Containers
```bash
docker compose up --build -d
```
*Wait ~5 seconds for PostgreSQL healthcheck to complete and database migrations/seeds to apply.*

### Step 3: Access Application
Open your browser at:
- **Web Application**: [http://localhost:3000](http://localhost:3000)
- **API Swagger UI**: [http://localhost:5123/swagger](http://localhost:5123/swagger)
- **Health Check**: [http://localhost:3000/health](http://localhost:3000/health)

---

## 3. End-to-End Evaluation Scenario

1. **Register Citizen**:
   - Navigate to [http://localhost:3000/register](http://localhost:3000/register).
   - Fill in citizen details (e.g., `ahmet.yilmaz@example.com` / `Citizen12345!`).
   - Click **Kayıt Ol** and automatically redirect to Login.
2. **Citizen Login & Submit Request**:
   - Log in with newly registered citizen credentials.
   - Click **Yeni Başvuru Yap** (or `/citizen/requests/new`).
   - Select category `Yol ve Asfalt Bakımı`, enter title `Nilüfer Bulvarı Asfalt Çökmesi`, fill in location and description.
   - Submit request. Inspect the generated tracking code and chronological timeline.
   - Click **Çıkış** (Logout) in the top-right header.
3. **Manager Login & Assignment**:
   - Log in as Manager: `manager@bursa.bel.tr` / `Demo12345!`.
   - On the Manager Dashboard, locate the newly submitted request.
   - Click **İncele** ($\rightarrow$ status becomes `Reviewing`).
   - Click **Görevlendir / Ata**: Select Department `Fen İşleri Dairesi Başkanlığı`, assign Employee `Mehmet Demir`, and set Priority `Yüksek` ($\rightarrow$ status becomes `Assigned`).
   - Log out.
4. **Field Employee Login & Resolution**:
   - Log in as Employee: `employee1@bursa.bel.tr` / `Demo12345!`.
   - In **Görevlerim**, open the assigned request.
   - Click **Çalışmayı Başlat** ($\rightarrow$ status becomes `InProgress`).
   - Click **Çalışmayı Tamamla**: Enter resolution note `Asfalt çökmesi saha ekiplerimizce sıcak asfalt dolgusu yapılarak onarıldı.` ($\rightarrow$ status becomes `Resolved`).
   - Log out.
5. **Manager Final Verification & Closure**:
   - Log in back as Manager: `manager@bursa.bel.tr` / `Demo12345!`.
   - Open the resolved request, review the employee's resolution note.
   - Click **Başvuruyu Kapat**: Enter closure note `Saha kontrolü onaylandı, talep kapatıldı.` ($\rightarrow$ status becomes `Closed`).
   - Observe the finalized timeline capturing all actors, status transitions, timestamps, and notes.

---

## 4. Local Development Demo Credentials

> [!NOTE]
> All credentials below are deterministic local development seed accounts intended solely for evaluator testing.

| Role | Email | Password | Department |
|---|---|---|---|
| **Manager** | `manager@bursa.bel.tr` | `Demo12345!` | Fen İşleri |
| **Employee 1** | `employee1@bursa.bel.tr` | `Demo12345!` | Fen İşleri |
| **Employee 2** | `employee2@bursa.bel.tr` | `Demo12345!` | Park ve Bahçeler |
| **Admin** | `admin@bursa.bel.tr` | `Demo12345!` | Administration |
| **Citizen** | *(Self-register from UI)* | *(User chosen)* | N/A |
