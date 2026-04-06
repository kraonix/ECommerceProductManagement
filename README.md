# ECommerce Microservices Platform 🛍️

A modernized enterprise-grade E-Commerce architecture built utilizing the .NET 10 microservices pattern and an Angular 17 Glassmorphic frontend. 

## 📖 Project Overview
This system represents a fully functional, highly scalable E-Commerce storefront and administrative backbone. It is designed to handle distinct user personas (Customers and Administrators) navigating completely distinct workflows, backed by independently deployed microservices to securely isolate business domains such as Identity, Inventory Cataloging, and Performance Analytics.

### 🔄 System Workflows & Application Pages
The frontend dynamically integrates with the backend through an Ocelot API Gateway to orchestrate the following primary user journeys:

1. **Authentication Identity Hub (Login Page)**
   - Operates as a unified state machine dynamically shifting between `LOGIN`, `SIGNUP`, `FORGOT PASSWORD`, and `RESET PASSWORD` flows without reloading the application.
   - Designed utilizing high-fidelity, premium Amazon-inspired Glassmorphism UI aesthetics.
   - Features intelligent RxJS HTTP Interceptors that silently capture `401 Unauthorized` requests from APIs to process invisible background JWT Refresh Token rotations automatically.

2. **Customer Storefront & Catalog (Home Page)**
   - Allows anonymous and authenticated users to browse sprawling product inventories natively piped from the `CatalogService`.
   - Connects user profiles to core purchasing workflows securely protected by JWT Bearer validations.

3. **Administrative Dashboard (Admin Panel)**
   - A restricted, highly protected reporting interface served concurrently by the `AdminReportingService`.
   - Evaluates system-wide operational metrics, aggregate product pipelines, and backend administrative telemetry securely.

## 🏗️ Architecture Stack

### Backend (.NET 10)
- **C# 12 / .NET 10 Web API** 
- **Ocelot API Gateway** (Port `5000`)
- **Entity Framework Core 10** (SQL Server ORM)
- **JWT (JSON Web Tokens)** logic with integrated Refresh Tokens
- **Asynchronous Messaging:** MassTransit / RabbitMQ (Background Workflows)

### Frontend (Angular)
- **Angular 17+** (Port `4200`)
- **RxJS State Mechanics** (Featuring intelligent HTTP token-rotation interceptors)
- **Glassmorphism UI** (Custom built geometric CSS abstraction modeling an Amazon-inspired dark theme style)

---

## 📂 Project Structure

All modules are explicitly scoped by their business domain:

```
📦 ECommerceProductManagement
 ┣ 📂 logs                     # Centralized traffic and identity logs
 ┣ 📂 src
 ┃ ┣ 📂 frontend               
 ┃ ┃ ┗ 📂 angular-app          # 🎨 Angular 17 UI Application
 ┃ ┣ 📂 gateway
 ┃ ┃ ┗ 📂 OcelotGateway        # 🌉 Central Routing & Audit Proxy (Port 5000)
 ┃ ┗ 📂 services
 ┃   ┣ 📂 AdminReportingService  # 📊 Dashboard & Analytics (Port 5040)
 ┃   ┣ 📂 CatalogService         # 📦 Inventory Management (Port 5020)
 ┃   ┣ 📂 IdentityService        # 🛡️ Secure Auth & Users (Port 5010)
 ┃   ┗ 📂 ProductWorkflowService # ⚙️ CQRS Event Workflows (Port 5030)
 ┗ 📜 run-all.bat              # 🚀 Universal Starter Script
```

---

## 🚀 Getting Started

### Prerequisites
1. **.NET 10 SDK** 
2. **Node.js** (latest LTS for Angular)
3. **SQL Server** (Local or Docker bound to localhost)

### Launching the Cluster
Instead of starting each microservice individually, we leverage the master orchestrator script provided at the root of the project:

```bat
# From an administrative command prompt:
> ./run-all.bat
```

This will automatically spin up multiple command terminal windows initializing the Gateway, all backend microservices, and serving the Angular developer environment globally.

---

## 🛡️ Authentication & Logging
This platform enforces strict `[Authorize]` attributes relying on stateless JWTs across downstream APIs.

### The Identity Pipeline
- Endpoints: Route via `http://localhost:5000/gateway/auth/*` 
- Features: Login, Registration, Token Revocation (Logout), Refresh Token Rotation, Password Reset.

### Advanced Action Logic Logging
This project utilizes a highly customized internal Middleware built directly into both the `OcelotGateway` and `IdentityService`. Operations log securely outside of standard execution folders (`bin`/`obj`) scaling to the top-level `/logs` directory natively parsing frontend JSON payload configurations:

**Log Snippet Example (`logs/identity_logs.txt`):**
```text
------------------------------------------------
customer@ecommerce.com - 2026-04-01 06:16:38 UTC
Login Execution - SUCCESS
-------------------------------------------------
admin@ecommerce.local - 2026-04-01 06:18:22 UTC
Logout / Token Revoke - SUCCESS
-------------------------------------------------
```

## ✨ Built With
- Clean Architecture methodologies.
- Global `RFC 7807` Standardized Problem details handling mappings.
- Completely natively injected custom Middlewares for network analysis.
