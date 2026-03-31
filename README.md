# Remp — Property Media Delivery & Showcase Platform

A backend API platform serving real estate photography companies and agents, providing professional property media delivery and showcase services.

---

## What It Does

**Admin (Photography Company):**

- Create and manage listing cases
- Upload photos, videos, floor plans, and VR tours to Azure Blob Storage
- Assign agents to listings and manage status transitions
- Generate shareable showcase pages

**Agent (Real Estate Agent):**

- View listing cases assigned to them
- Select display media (max 10 images)
- Set cover/hero image
- Add contact information
- Share the showcase page with buyer clients via a public URL

---

## Tech Stack

| Layer                | Technology                                         |
| -------------------- | -------------------------------------------------- |
| Web API Framework    | .NET 8 + ASP.NET Core                              |
| Primary Database     | SQL Server + Entity Framework Core                 |
| Audit / Log Database | MongoDB Atlas                                      |
| File Storage         | Azure Blob Storage (SAS token access)              |
| Authentication       | ASP.NET Identity + JWT                             |
| Object Mapping       | AutoMapper 13                                      |
| Input Validation     | FluentValidation                                   |
| Unit Testing         | xUnit + Moq                                        |
| API Documentation    | Swagger / OpenAPI                                  |
| Containerisation     | Docker + docker-compose                            |
| Cloud Deployment     | Azure App Service + Azure SQL + Azure Blob Storage |
| CI/CD                | GitHub Actions                                     |
| Email                | Gmail SMTP via MailKit                             |

---

## Architecture

Layered architecture following SOLID principles:

```
Remp.API → Remp.Service → Remp.Repository → Remp.DataAccess → Remp.Models
                                                              ↑
                                               Remp.Common (all layers)
```

| Project           | Responsibility                               |
| ----------------- | -------------------------------------------- |
| `Remp.Models`     | Entities, Enums, Constants                   |
| `Remp.DataAccess` | EF Core DbContext, Migrations, MongoDB setup |
| `Remp.Repository` | Repository Pattern, UnitOfWork, transactions |
| `Remp.Service`    | Business logic, DTOs, Validators, AutoMapper |
| `Remp.Common`     | Custom exceptions, ApiResponse helper        |
| `Remp.API`        | Controllers, Middleware, app bootstrap       |

---

## User Roles

| Role                         | Description                                                     |
| ---------------------------- | --------------------------------------------------------------- |
| `PhotographyCompany` (Admin) | Full system access — manages listings, media, agents            |
| `Agent`                      | Restricted access — view assigned listings, select media, share |

---

## API Endpoints

### Authentication

| Method | Path               | Auth   | Description           |
| ------ | ------------------ | ------ | --------------------- |
| POST   | /auth/login        | Public | Login, returns JWT    |
| GET    | /users/me          | All    | Get current user info |
| PUT    | /users/me/password | All    | Update password       |

### Agents

| Method | Path                             | Auth  | Description                           |
| ------ | -------------------------------- | ----- | ------------------------------------- |
| POST   | /agents                          | Admin | Create agent + send credentials email |
| GET    | /agents                          | Admin | Get agents under current admin        |
| GET    | /agents/search?email=            | Admin | Search agent by email                 |
| POST   | /agents/{id}/photography-company | Admin | Link agent to company                 |

### Listings

| Method | Path                        | Auth        | Description                             |
| ------ | --------------------------- | ----------- | --------------------------------------- |
| POST   | /listings                   | Admin       | Create listing case                     |
| GET    | /listings                   | Admin/Agent | Get listing list (paginated + filtered) |
| GET    | /listings/{id}              | Admin/Agent | Get listing details                     |
| PUT    | /listings/{id}              | Admin       | Update listing                          |
| DELETE | /listings/{id}              | Admin       | Soft delete listing                     |
| PATCH  | /listings/{id}/status       | Admin       | Update status (forward only)            |
| POST   | /listings/{id}/assign-agent | Admin       | Assign agent to listing                 |
| POST   | /listings/{id}/publish      | Admin/Agent | Generate shareable URL                  |
| GET    | /listings/preview/{token}   | Public      | Public showcase page data               |

### Media

| Method | Path                       | Auth        | Description                   |
| ------ | -------------------------- | ----------- | ----------------------------- |
| POST   | /listings/{id}/media       | Admin       | Upload media files            |
| GET    | /listings/{id}/media       | Admin/Agent | Get all media grouped by type |
| DELETE | /media/{id}                | Admin       | Delete media file             |
| PUT    | /listings/{id}/cover-image | Admin/Agent | Set hero image                |
| GET    | /media/{id}/download       | Admin/Agent | Download single file          |
| GET    | /listings/{id}/download    | Admin/Agent | ZIP download all media        |

### Selection & Contacts

| Method | Path                           | Auth        | Description                   |
| ------ | ------------------------------ | ----------- | ----------------------------- |
| PUT    | /listings/{id}/selected-media  | Agent       | Select display media (max 10) |
| GET    | /listings/{id}/final-selection | Admin/Agent | Get selected media            |
| POST   | /listings/{id}/contacts        | Agent       | Add contact                   |
| GET    | /listings/{id}/contacts        | Admin/Agent | Get contacts                  |

---

## Status Transitions

```
Created → Pending → In Review → Delivered
```

Forward-only. Every transition is logged to MongoDB CaseHistory.

---

## Running Locally

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Node.js (for Azurite)

### 1. Clone the repo

```bash
git clone https://github.com/your-repo/Recam-backend.git
cd Recam-backend
```

### 2. Start local services

**SQL Server:**

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 --name remp-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**MongoDB:**

```bash
docker run -d --name remp-mongo -p 27017:27017 mongo:7
```

**Azurite (Blob Storage emulator):**

```bash
npm install -g azurite
azurite --skipApiVersionCheck --silent --location ~/.azurite --debug ~/.azurite/debug.log
```

### 3. Configure `appsettings.Development.json`

Create `Remp.API/appsettings.Development.json` (gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RempDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True"
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RempDb",
    "CaseHistoryCollection": "CaseHistory",
    "UserActivityLogCollection": "UserActivityLog"
  },
  "JwtSettings": {
    "SecretKey": "your-local-secret-key-at-least-32-characters",
    "Issuer": "remp-api",
    "Audience": "remp-client",
    "ExpiryMinutes": 60
  },
  "BlobStorageSettings": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "remp-media"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-gmail@gmail.com",
    "Password": "your-app-password"
  },
  "FrontendUrl": "http://localhost:5173",
  "AllowedOrigins": ["http://localhost:5173"]
}
```

### 4. Run the API

```bash
dotnet run --project Remp.API
```

API runs at `http://localhost:5096`. Swagger available at `http://localhost:5096/swagger`.

### 5. Default seeded accounts

| Role  | Email          | Password   |
| ----- | -------------- | ---------- |
| Admin | admin@remp.com | Admin@123! |
| Agent | agent@remp.com | Agent@123! |

---

## Running with Docker Compose

```bash
docker-compose up --build
```

API runs at `http://localhost:8080`.

---

## Running Tests

```bash
dotnet test
```

---

## Production Deployment

Deployed via GitHub Actions on push to `main`:

1. Build + test
2. Publish
3. Deploy to Azure App Service

### Azure Infrastructure

- **App Service** — `remp-api-free` (Australia East)
- **Azure SQL** — `remp-db` (Basic tier)
- **Azure Blob Storage** — `rempstoragfree` (LRS, SAS token access)
- **MongoDB Atlas** — `remp-cluster` (M0 Free, Sydney)

### Required Azure App Service environment variables

| Name                                         | Description                     |
| -------------------------------------------- | ------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                     | `Production`                    |
| `JwtSettings__SecretKey`                     | JWT signing key (min 32 chars)  |
| `JwtSettings__Issuer`                        | `remp-api`                      |
| `JwtSettings__Audience`                      | `remp-client`                   |
| `BlobStorageSettings__ConnectionString`      | Azure Blob connection string    |
| `BlobStorageSettings__ContainerName`         | `remp-media`                    |
| `MongoDbSettings__ConnectionString`          | MongoDB Atlas connection string |
| `MongoDbSettings__DatabaseName`              | `remp-db`                       |
| `MongoDbSettings__CaseHistoryCollection`     | `CaseHistory`                   |
| `MongoDbSettings__UserActivityLogCollection` | `UserActivityLog`               |
| `Email__Host`                                | `smtp.gmail.com`                |
| `Email__Port`                                | `587`                           |
| `Email__Username`                            | Gmail address                   |
| `Email__Password`                            | Gmail app password              |
| `FrontendUrl`                                | Frontend domain                 |
| `AllowedOrigins__0`                          | Frontend origin for CORS        |

### Required Azure App Service connection strings

| Name                | Type     | Description                 |
| ------------------- | -------- | --------------------------- |
| `DefaultConnection` | SQLAzure | Azure SQL connection string |

---

## Security Notes

- `appsettings.Development.json` is gitignored — never commit secrets
- Blob files served via time-limited SAS tokens (1 year expiry)
- Swagger disabled in Production
- JWT required on all endpoints except `POST /auth/login` and `GET /listings/preview/{token}`
