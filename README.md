# Obsidian Enterprise — ECommerce Product Management System 🛍️

A production-grade, full-stack e-commerce platform built on **.NET 10 microservices** and **Angular 21** with a premium Glassmorphism dark UI. Designed for enterprise product lifecycle management — from creation and enrichment to approval, publishing, and customer storefront delivery.

---

## 📖 Overview

The system supports four distinct user roles across two separate workflows:

| Role | Access |
|---|---|
| **Admin** | Full system access — approvals, publishing, reporting, user management |
| **ProductManager** | Create products, manage pricing/inventory, submit for review |
| **ContentExecutive** | Upload and manage product media, enrich product attributes |
| **Customer** | Browse published products, view details, manage cart |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Angular 21 SPA                        │
│              http://localhost:4200                       │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTP
┌──────────────────────▼──────────────────────────────────┐
│              Ocelot API Gateway                          │
│              http://localhost:5000                       │
│   Rate limiting (auth) · CORS · Request logging         │
└──┬──────────┬──────────┬──────────┬────────────────────┘
   │          │          │          │
   ▼          ▼          ▼          ▼
Identity   Catalog   Workflow   Reporting
:5010      :5020      :5030      :5040
```

### Backend Services

| Service | Port | Responsibility |
|---|---|---|
| **IdentityService** | 5010 | JWT auth, refresh tokens, user management, rate limiting |
| **CatalogService** | 5020 | Product CRUD, media upload/delete, static file serving |
| **ProductWorkflowService** | 5030 | Pricing, inventory, approval workflow, audit logging |
| **AdminReportingService** | 5040 | Dashboard metrics, CSV export, audit trail |
| **OcelotGateway** | 5000 | Routing, CORS, request logging |

### Frontend Pages

| Route | Role | Description |
|---|---|---|
| `/auth/login` | All | Login · Signup · Forgot/Reset Password |
| `/admin/dashboard` | Admin/PM/CE | KPI cards, product chart, activity feed |
| `/admin/catalog` | Admin/PM/CE | Product table with inline edit drawer |
| `/admin/catalog/new` | Admin/PM | 6-step product creation wizard |
| `/admin/preview/:id` | Admin/PM/CE | Full storefront preview with real images |
| `/admin/audit` | Admin/PM | Product audit log viewer |
| `/admin/reports` | Admin | Analytics, CSV export |
| `/customer/products` | Customer | Storefront product grid |
| `/customer/product/:id` | Customer | Product detail with image gallery |
| `/customer/cart` | Customer | Shopping cart |

---

## 🔄 Product Lifecycle

```
Draft → In Enrichment → Ready for Review → Approved → Published
                                        ↘ Rejected
                                                    → Archived
```

1. **ProductManager** creates product (Draft)
2. **ContentExecutive** uploads images, enriches attributes (In Enrichment)
3. **ProductManager** sets pricing/inventory, submits for review
4. **Admin** approves or rejects
5. Approved products become visible to customers

---

## ✨ Key Features

### Authentication
- JWT Bearer tokens (8-hour expiry) + Refresh tokens (7-day expiry)
- Silent background token rotation via Angular HTTP interceptor
- BCrypt password hashing
- Rate limiting: 30 auth requests/minute per IP (enforced at IdentityService)
- Password reset flow (token-based)

### Product Management
- Full CRUD with SKU uniqueness validation
- 6-step creation wizard (Basic Info → Media → Pricing → Inventory → Review → Publish)
- Inline edit drawer on catalog page — edit all fields + manage images
- Media upload with base64 encoding, stored to disk, served as static files
- Per-image delete + delete all images
- Real product images shown on customer storefront and admin preview

### Workflow & Approvals
- Full state machine: Draft → In Enrichment → Ready for Review → Approved/Rejected → Published/Archived
- Remarks required for Rejection and Archive
- Every workflow action writes an audit log entry

### Reporting & Audit
- Dashboard: live product counts, pending approvals, low stock alerts (≤5 units)
- CSV export with real data (not hardcoded)
- Audit log: chronological history per product, searchable

### UI/UX
- Obsidian dark design system (Glassmorphism, Manrope + Inter fonts)
- Collapsible sidebar — click the logo icon to toggle
- Slide-out edit drawer for product management
- Image gallery with thumbnail strip on product detail
- Responsive layouts across all admin and customer pages

---

## 📂 Project Structure

```
ECommerceProductManagement/
├── docs/                          # Architecture diagrams, sprint plan
├── logs/                          # gateway_logs.txt, identity_logs.txt, upload_logs.txt
├── src/
│   ├── frontend/angular-app/      # Angular 21 SPA
│   │   ├── src/app/
│   │   │   ├── core/
│   │   │   │   ├── guards/        # authGuard (JWT expiry check), roleGuard
│   │   │   │   ├── interceptors/  # authInterceptor (token refresh + queue)
│   │   │   │   ├── layout/        # Admin shell (sidebar + topbar + footer)
│   │   │   │   └── services/      # ApiService, CartService
│   │   │   └── features/          # One folder per page
│   ├── gateway/OcelotGateway/     # Ocelot routing + logging middleware
│   └── services/
│       ├── AdminReportingService/ # Port 5040
│       ├── CatalogService/        # Port 5020 — includes wwwroot/uploads/
│       ├── IdentityService/       # Port 5010
│       └── ProductWorkflowService/# Port 5030
├── tests/                         # NUnit unit tests for all 4 services
├── run-all.bat                    # Launches all services + frontend
└── ECommerceProductManagement.slnx
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js LTS](https://nodejs.org/) (for Angular)
- **SQL Server** (local instance or Docker on `localhost`)

### Launch Everything

```bat
run-all.bat
```

This opens 6 terminal windows:

| Window | URL |
|---|---|
| Identity Service | http://localhost:5010/swagger |
| Catalog Service | http://localhost:5020/swagger |
| Workflow Service | http://localhost:5030/swagger |
| Reporting Service | http://localhost:5040/swagger |
| API Gateway | http://localhost:5000 |
| Angular Frontend | http://localhost:4200 |

### Database

Each service auto-migrates its own database on startup. Seed data (22 products across 2 categories) is loaded automatically by CatalogService on first run.

### Default Accounts

Register via the signup form. Available roles:
- `Admin`
- `ProductManager`
- `ContentExecutive`
- `Customer`

---

## 🛡️ Security

- All API endpoints require JWT Bearer authentication
- Role-based authorization enforced at both gateway and service level
- Auth endpoints rate-limited to 30 req/min per IP
- CORS restricted to `http://localhost:4200`
- Client-side JWT expiry validation in route guards
- Concurrent 401 handling — queued requests retry after token refresh

---

## 📊 API Gateway Routes

| Upstream (via Gateway) | Downstream Service |
|---|---|
| `GET/POST /gateway/catalog/products` | CatalogService :5020 |
| `GET/PUT/DELETE /gateway/catalog/products/{id}` | CatalogService :5020 |
| `GET /gateway/catalog/categories` | CatalogService :5020 |
| `POST /gateway/catalog/products/{id}/media` | CatalogService :5020 |
| `DELETE /gateway/catalog/products/{id}/media/{mediaId}` | CatalogService :5020 |
| `POST/PUT /gateway/workflow/products/{id}/*` | WorkflowService :5030 |
| `GET /gateway/admin/reports/dashboard` | ReportingService :5040 |
| `GET /gateway/admin/reports/export` | ReportingService :5040 |
| `GET/POST /gateway/admin/audit/*` | ReportingService :5040 |
| `POST /gateway/auth/*` | IdentityService :5010 |
| `GET /health/{service}` | Respective service `/health` |

---

## 🧪 Tests

Unit tests for all 4 backend services using **NUnit** + **EF Core InMemory** + **Moq**:

```bash
dotnet test
```

Test coverage includes:
- Auth: signup, login, duplicate email, invalid role, wrong password
- Catalog: product search, create (Draft status), duplicate SKU, invalid media type
- Workflow: pricing validation (sale ≤ MRP), submit for review, status updates
- Reporting: CSV export, audit log chronological order

---

## 📝 Logging

| Log File | Contents |
|---|---|
| `logs/gateway_logs.txt` | All gateway requests with user email, method, path, status |
| `logs/identity_logs.txt` | Login/logout/token events |
| `logs/upload_logs.txt` | Media upload diagnostics (file path, DB record, byte count) |

---

## 🔧 Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, C# 12, ASP.NET Core Web API |
| ORM | Entity Framework Core 10, SQL Server |
| Gateway | Ocelot 24 |
| Auth | JWT Bearer, BCrypt.Net, ASP.NET Core Rate Limiting |
| Frontend | Angular 21, TypeScript 5.9, RxJS 7.8 |
| State | Angular Signals (cart), ChangeDetectorRef |
| Styling | SCSS, Glassmorphism design system (Manrope + Inter) |
| Testing | NUnit 4, Moq, EF Core InMemory |
| Dev Tools | Swagger/OpenAPI on all services |
