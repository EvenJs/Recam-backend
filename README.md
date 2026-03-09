# Property Media Delivery & Showcase Platform — Requirements Document

**Version:** v1.3  
**Date:** 2026-03-09  
**Tech Stack:** .NET 8 · ASP.NET Core · EF Core · SQL Server · MongoDB · Azure Blob Storage · JWT · xUnit

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technical Architecture](#2-technical-architecture)
3. [Backend Solution Structure](#3-backend-solution-structure)
4. [Engineering Standards & Constraints](#4-engineering-standards--constraints)
5. [User Roles & Permissions](#5-user-roles--permissions)
6. [Data Storage Design](#6-data-storage-design)
7. [Feature Modules](#7-feature-modules)
8. [API Endpoints Overview](#8-api-endpoints-overview)
9. [Status Transition Rules](#9-status-transition-rules)
10. [Task List](#10-task-list)

---

## 1. Project Overview

This platform serves **real estate photography companies (PhotographyCompany / Admin)** and **real estate agents (Agent)**, providing professional property media delivery and showcase services.

- **Admin Side**: Create listing cases, upload photos / videos / floor plans / VR Tours, manage media assets, update statuses, and generate final showcase pages.
- **Agent Side**: Browse listing cases assigned to them, select display media, edit the showcase page, and share it with buyer clients.

---

## 2. Technical Architecture

| Layer                          | Technology                                         |
| ------------------------------ | -------------------------------------------------- |
| Web API Framework              | .NET 8 + ASP.NET Core                              |
| Primary Database               | SQL Server + Entity Framework Core                 |
| Audit / Log Database           | MongoDB (operation logs, change history)           |
| File Storage                   | Azure Blob Storage (images, videos, floor plans)   |
| Authentication & Authorization | ASP.NET Identity + JWT                             |
| Object Mapping                 | AutoMapper                                         |
| Input Validation               | FluentValidation                                   |
| Unit Testing                   | xUnit + Moq                                        |
| API Documentation              | Swagger / OpenAPI                                  |
| Containerization               | Docker + docker-compose                            |
| Cloud Deployment               | Azure App Service + Azure SQL + Azure Blob Storage |

**Architecture Principles:** Layered architecture (Controller → Service → Repository → DB), adhering to SOLID principles.

---

## 3. Backend Solution Structure

```
Remp.Solution/
├── Remp.Models/                    # Data Model Layer
│   ├── Entities/                   # EF Core entity classes (ListingCase, MediaAsset, User, etc.)
│   ├── Enums/                      # Enumerations (MediaType, PropertyType, SaleType, ListingStatus, etc.)
│   └── Constants/                  # System-wide constants (role names, status strings, etc.)
│
├── Remp.DataAccess/                # Data Access Configuration Layer
│   ├── Data/                       # EF Core DbContext (AppDbContext), migrations
│   └── Collections/                # MongoDB collection configuration & index setup
│
├── Remp.Repository/                # Repository Layer (Database Access Abstraction)
│   ├── Interfaces/                 # Repository interfaces (IListingCaseRepository, IMediaAssetRepository, etc.)
│   ├── Repositories/               # Concrete EF Core & MongoDB repository implementations
│   └── Common/                     # Shared transaction helpers, UnitOfWork, DbCommit utilities
│
├── Remp.Service/                   # Application Service Layer
│   ├── Interfaces/                 # Service interfaces (IListingCaseService, IMediaService, etc.)
│   ├── Services/                   # Business logic implementations
│   ├── DTOs/                       # Request & Response Data Transfer Objects
│   ├── Mappers/                    # AutoMapper profiles (Entity ↔ DTO mappings)
│   └── Validators/                 # FluentValidation validators for all request DTOs
│
├── Remp.Common/                    # Shared / Cross-Cutting Layer
│   ├── Exceptions/                 # Custom exception classes (NotFoundException, ForbiddenException, etc.)
│   ├── Extensions/                 # Extension methods (IServiceCollection, string, etc.)
│   ├── Helpers/                    # Static utility helpers (password generator, slug builder, etc.)
│   └── Utilities/                  # Other utilities (file type checker, watermark processor, etc.)
│
└── Remp.API/                       # Presentation Layer
    ├── Controllers/                # ASP.NET Core API controllers
    ├── Middlewares/                # Exception handling middleware, request logging middleware
    ├── Program.cs                  # App entry point — DI registration, middleware pipeline
    └── appsettings.json            # Configuration (connection strings, JWT settings, Blob config)
```

### Layer Dependency Rules

```
Remp.API
  └── depends on → Remp.Service
                      └── depends on → Remp.Repository
                                          └── depends on → Remp.DataAccess
                                                              └── depends on → Remp.Models
                      └── depends on → Remp.Models
  └── depends on → Remp.Common   (all layers may reference Common)
```

> **Rules:**
>
> - Inner layers must **never** reference outer layers (e.g. `Remp.Models` cannot reference `Remp.Service`).
> - `Remp.Common` is the only layer that **all other layers** may freely reference.
> - Controllers must **never** contain business logic — delegate entirely to Services.
> - Repositories must **never** contain business logic — only data access operations.
> - DTOs must **never** be used in the Repository layer — only Entities cross the Repository boundary.

### Project Responsibilities at a Glance

| Project           | Responsibility                | Key Contents                                    |
| ----------------- | ----------------------------- | ----------------------------------------------- |
| `Remp.Models`     | Define what data looks like   | Entities, Enums, Constants                      |
| `Remp.DataAccess` | Configure how data is stored  | DbContext, Migrations, MongoDB setup            |
| `Remp.Repository` | Abstract data access          | CRUD operations, transactions, UnitOfWork       |
| `Remp.Service`    | Implement business rules      | Services, DTOs, Validators, AutoMapper profiles |
| `Remp.Common`     | Share cross-cutting utilities | Custom exceptions, helpers, extensions          |
| `Remp.API`        | Expose HTTP endpoints         | Controllers, Middleware, app bootstrap          |

---

## 4. Engineering Standards & Constraints

These standards are **mandatory** for all code in this project. Every task must be implemented in compliance with the following rules.

### 4.1 Exception Handling — Exception Middleware

All exceptions **must** be handled centrally through a global Exception Middleware. Controllers and services must never catch and suppress exceptions silently.

**Requirements:**

- Register a custom `ExceptionHandlingMiddleware` in the ASP.NET Core pipeline.
- The middleware must catch all unhandled exceptions and return a **unified error response format**:

```json
{
  "statusCode": 400,
  "message": "Validation failed.",
  "errors": ["Title must not be empty.", "Address is required."]
}
```

- Map exception types to HTTP status codes:

| Exception Type                           | HTTP Status               |
| ---------------------------------------- | ------------------------- |
| `ValidationException` (FluentValidation) | 400 Bad Request           |
| `UnauthorizedAccessException`            | 401 Unauthorized          |
| `ForbiddenException` (custom)            | 403 Forbidden             |
| `NotFoundException` (custom)             | 404 Not Found             |
| `InvalidOperationException`              | 409 Conflict              |
| All other exceptions                     | 500 Internal Server Error |

- In Development environment, include stack trace in the response for debugging.
- In Production environment, log the full exception internally; return only a generic message to the client.

### 4.2 Dependency Injection — Service Lifetime Management

All services, repositories, and utilities **must** be registered and resolved through the built-in ASP.NET Core DI container. Direct instantiation with `new` is forbidden for injectable components.

**Lifetime rules:**

| Lifetime    | Usage                                                                           |
| ----------- | ------------------------------------------------------------------------------- |
| `Scoped`    | Services and Repositories (default for EF Core DbContext and business services) |
| `Singleton` | Configuration helpers, caching services, lightweight stateless utilities        |
| `Transient` | Lightweight, stateless services used briefly per operation                      |

**Requirements:**

- Register all services in `Program.cs` or via dedicated extension methods (e.g. `services.AddApplicationServices()`).
- Program to interfaces, not concrete implementations — every service must have a corresponding interface (e.g. `IListingService` / `ListingService`).
- Azure Blob Storage service, Email Sender, and MongoDB repositories must all be registered via DI.

### 4.3 Transactions — Data Consistency Across Multiple Tables

Any operation that writes to **two or more tables** must be wrapped in a database transaction to guarantee atomicity.

**Requirements:**

- Use EF Core's `IDbContextTransaction` via `_context.Database.BeginTransactionAsync()`.
- On success, call `await transaction.CommitAsync()`.
- On failure, call `await transaction.RollbackAsync()` and re-throw the exception for the middleware to handle.

**Operations that require transactions:**

| Operation                     | Tables Involved                                                               |
| ----------------------------- | ----------------------------------------------------------------------------- |
| Create Listing Case           | ListingCase + MongoDB CaseHistory                                             |
| Delete Listing Case           | ListingCase + MediaAsset + CaseContact + AgentListingCase                     |
| Upload Media                  | MediaAsset + (ListingCase hero update if applicable)                          |
| Agent Select / Set Hero Media | MediaAsset (IsSelect / IsHero flags) + MongoDB UserActivityLog                |
| Assign Agent to Listing       | AgentListingCase + MongoDB UserActivityLog                                    |
| Update Listing Status         | ListingCase + MongoDB CaseHistory                                             |
| Create Agent Account          | User:IdentityUser + Agent + AgentPhotographyCompany + MongoDB UserActivityLog |

**Example pattern:**

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // SQL Server writes
    _context.ListingCases.Add(listingCase);
    _context.StatusHistory.Add(statusEntry);
    await _context.SaveChangesAsync();

    // MongoDB audit log
    await _caseHistoryRepo.InsertAsync(caseHistory);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 4.4 Input Validation — FluentValidation

All incoming request data **must** be validated using FluentValidation. Manual validation logic inside controllers or services is not permitted.

**Requirements:**

- Create a dedicated `Validator` class for every request DTO (e.g. `CreateListingCaseRequestValidator`).
- Register all validators via `services.AddValidatorsFromAssemblyContaining<T>()`.
- Integrate FluentValidation with ASP.NET Core's model binding pipeline so validation errors are automatically caught by the Exception Middleware.
- Validation rules must cover: required fields, string length limits, allowed enum values, numeric ranges, and email format where applicable.

**Example:**

```csharp
public class CreateListingCaseRequestValidator : AbstractValidator<CreateListingCaseRequest>
{
    public CreateListingCaseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.");

        RuleFor(x => x.PropertyType)
            .IsInEnum().WithMessage("Invalid property type.");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or more.");
    }
}
```

### 4.5 API Documentation — XML Comments

All controller actions and public service interfaces **must** include XML documentation comments. Swagger is configured to read these comments and display them in the API docs.

**Requirements:**

- Enable XML documentation output in the project `.csproj`:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

- Configure Swagger to include the XML file:

```csharp
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
options.IncludeXmlComments(xmlPath);
```

- Every controller action must include at minimum: `<summary>`, `<param>`, and `<response>` tags.

**Example:**

```csharp
/// <summary>
/// Creates a new listing case. Accessible by PhotographyCompany (Admin) only.
/// </summary>
/// <param name="request">The listing case creation request body.</param>
/// <returns>The created listing case with its generated ID and initial status.</returns>
/// <response code="201">Listing case created successfully.</response>
/// <response code="400">Validation failed — check request body.</response>
/// <response code="401">Authentication token is missing or invalid.</response>
/// <response code="403">Caller does not have Admin privileges.</response>
[HttpPost]
[Authorize(Roles = "PhotographyCompany")]
[ProducesResponseType(typeof(ApiResponse<ListingCaseDto>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateListingCase([FromBody] CreateListingCaseRequest request) { ... }
```

---

## 5. User Roles & Permissions

### 5.1 Role Definitions

| Role                       | Description                                                                                 |
| -------------------------- | ------------------------------------------------------------------------------------------- |
| PhotographyCompany (Admin) | Photography company account with highest system privileges; manages all listings and agents |
| Agent                      | Real estate agent; can only view listing cases assigned to them                             |

### 5.2 Permission Matrix

| Feature                               | Admin                  | Agent                       |
| ------------------------------------- | ---------------------- | --------------------------- |
| Create Listing Case                   | ✅                     | ❌                          |
| View Listing Case List                | ✅ (own listings only) | ✅ (assigned listings only) |
| Edit Listing Case Info                | ✅                     | ✅                          |
| Delete Listing Case                   | ✅                     | ❌                          |
| Upload Media Files                    | ✅                     | ❌                          |
| Delete Media Files                    | ✅                     | ❌                          |
| Select Display Media (Selected Media) | View result only       | ✅                          |
| Set Cover Image (Hero Image)          | ✅                     | ✅                          |
| Edit Showcase Page                    | ✅                     | ✅                          |
| Preview Showcase Page                 | ✅                     | ✅                          |
| Download Media Resources (ZIP)        | ✅                     | ✅                          |
| Generate Shareable Link               | ✅                     | ✅                          |
| Change Listing Status                 | ✅                     | ❌                          |
| Create Agent Account                  | ✅                     | ❌                          |
| Manage Agent List                     | ✅                     | ❌                          |
| Assign Agent to Listing               | ✅                     | ❌                          |
| Add Case Contact (CaseContact)        | ❌                     | ✅                          |
| Change Own Password                   | ✅                     | ✅                          |

---

## 6. Data Storage Design

### 6.1 SQL Server — Entity Relationship Overview

```
User:IdentityUser
  ├── Is ──────────────────► PhotographyCompany (PK FK: Id)
  ├── Is ──────────────────► Agent (PK FK: Id)
  ├── Created By ──────────► ListingCase (FK: UserId)
  └── Uploaded By ─────────► MediaAsset (FK: UserId)

PhotographyCompany
  └── Has ─────────────────► AgentPhotographyCompany (FK: PhotographyCompanyId)

Agent
  └── Has ─────────────────► AgentPhotographyCompany (FK: AgentId)
  └── Assigned to ─────────► AgentListingCase (FK: AgentId)

ListingCase
  ├── Has ─────────────────► AgentListingCase (FK: ListingCaseId)
  ├── Has ─────────────────► CaseContact (FK: ListingCaseId)
  └── Has ─────────────────► MediaAsset (FK: ListingCaseId)

UserIdentityRole  ←──  UserIdentityUserRole  ──►  User:IdentityUser
```

---

### 6.2 SQL Server — Table Definitions

#### User : IdentityUser

Extends ASP.NET Identity's `IdentityUser`. Base identity table for all system users.

| Field                           | Type     | Description                   |
| ------------------------------- | -------- | ----------------------------- |
| (inherited IdentityUser fields) | —        | Id, Email, PasswordHash, etc. |
| IsDeleted                       | boolean  | Soft delete flag              |
| CreatedAt                       | DateTime | Account creation timestamp    |

---

#### PhotographyCompany

Extends `User:IdentityUser`. Represents a photography company (Admin) account.

| Key    | Field                  | Type   | Description                |
| ------ | ---------------------- | ------ | -------------------------- |
| PK, FK | Id                     | string | Inherits from IdentityUser |
|        | PhotographyCompanyName | string | Company display name       |

---

#### Agent

Extends `User:IdentityUser`. Represents a real estate agent account.

| Key    | Field          | Type   | Description                |
| ------ | -------------- | ------ | -------------------------- |
| PK, FK | Id             | string | Inherits from IdentityUser |
|        | AgentFirstName | string | Agent's first name         |
|        | AgentLastName  | string | Agent's last name          |
|        | AvatarUrl      | string | Profile image URL          |
|        | CompanyName    | string | Agent's company name       |

---

#### AgentPhotographyCompany

Join table linking Agent to PhotographyCompany (many-to-many).

| Key          | Field                | Type   | Description                |
| ------------ | -------------------- | ------ | -------------------------- |
| Composite PK | AgentId              | string | FK → Agent.Id              |
| Composite PK | PhotographyCompanyId | string | FK → PhotographyCompany.Id |

---

#### AgentListingCase

Join table linking Agent to ListingCase (assignment).

| Key          | Field         | Type   | Description         |
| ------------ | ------------- | ------ | ------------------- |
| Composite FK | AgentId       | string | FK → Agent.Id       |
| Composite FK | ListingCaseId | int    | FK → ListingCase.Id |

---

#### ListingCase

Core listing case table. Created by a PhotographyCompany, assigned to Agent(s).

| Key | Field          | Type      | Description                                         |
| --- | -------------- | --------- | --------------------------------------------------- |
| PK  | Id             | int       | Primary key                                         |
|     | Title          | string    | Listing title                                       |
|     | Description    | string    | Property description                                |
|     | Street         | string    | Street address                                      |
|     | City           | string    | City                                                |
|     | State          | string    | State                                               |
|     | Postcode       | int       | Postcode                                            |
|     | Longitude      | decimal   | GPS longitude                                       |
|     | Latitude       | decimal   | GPS latitude                                        |
|     | Price          | double    | Listing price                                       |
|     | Bedrooms       | int       | Number of bedrooms                                  |
|     | Bathrooms      | int       | Number of bathrooms                                 |
|     | Garages        | int       | Number of garages                                   |
|     | FloorArea      | double    | Floor area (sqm)                                    |
|     | CreatedAt      | datetime  | Creation timestamp                                  |
|     | IsDeleted      | boolean   | Soft delete flag                                    |
|     | PropertyType   | int(enum) | House=1 / Unit=2 / Townhouse=3 / Villa=4 / Others=5 |
|     | SaleCategory   | int(enum) | ForSale=1 / ForRent=2 / Auction=3                   |
|     | ListcaseStatus | int(enum) | Created=1 / Pending=2 / Delivered=3                 |
| FK  | UserId         | string    | FK → User:IdentityUser.Id (Created By)              |

---

#### CaseContact

Contact persons associated with a listing case (added by Agent).

| Key | Field         | Type   | Description          |
| --- | ------------- | ------ | -------------------- |
| PK  | ContactId     | int    | Primary key          |
|     | FirstName     | string | Contact first name   |
|     | LastName      | string | Contact last name    |
|     | CompanyName   | string | Contact company name |
|     | ProfileUrl    | string | Profile image URL    |
|     | Email         | string | Contact email        |
|     | PhoneNumber   | string | Contact phone number |
| FK  | ListingCaseId | int    | FK → ListingCase.Id  |

---

#### MediaAsset

Stores metadata for all uploaded media files (image, video, floor plan, VR tour).

| Key | Field         | Type      | Description                                  |
| --- | ------------- | --------- | -------------------------------------------- |
| PK  | Id            | int       | Primary key                                  |
|     | MediaType     | int(enum) | Picture=1 / Video=2 / FloorPlan=3 / VRTour=4 |
|     | MediaUrl      | string    | Azure Blob Storage URL                       |
|     | UploadedAt    | datetime  | Upload timestamp                             |
|     | IsSelect      | boolean   | Whether selected by Agent for display        |
|     | IsHero        | boolean   | Whether set as cover/hero image              |
|     | IsDeleted     | boolean   | Soft delete flag                             |
| FK  | ListingCaseId | int       | FK → ListingCase.Id                          |
| FK  | UserId        | string    | FK → User:IdentityUser.Id (Uploaded By)      |

---

#### UserIdentityRole

ASP.NET Identity role table.

| Field    | Type   | Description                            |
| -------- | ------ | -------------------------------------- |
| RoleName | String | Role name (PhotographyCompany / Agent) |

---

#### UserIdentityUserRole

ASP.NET Identity join table linking users to roles.

| Field  | Type   | Description            |
| ------ | ------ | ---------------------- |
| RoleId | String | FK → UserIdentityRole  |
| UserId | String | FK → User:IdentityUser |

---

### 6.3 Enum Reference

| Enum               | Values                                          |
| ------------------ | ----------------------------------------------- |
| **ListcaseStatus** | Created=1, Pending=2, Delivered=3               |
| **MediaType**      | Picture=1, Video=2, FloorPlan=3, VRTour=4       |
| **PropertyType**   | House=1, Unit=2, Townhouse=3, Villa=4, Others=5 |
| **SaleCategory**   | ForSale=1, ForRent=2, Auction=3                 |
| **ChangeAction**   | Created, Read, Updated, Deleted, Shared         |
| **OrderStatus**    | Pending, Scheduled, Preparing, Delivered        |

---

### 6.4 MongoDB Collections

| Collection      | Description                                                                                   |
| --------------- | --------------------------------------------------------------------------------------------- |
| CaseHistory     | Full change history for each listing case (field edits, status transitions, ChangeAction log) |
| UserActivityLog | User operation logs (login, register, upload, download, etc.)                                 |

> **Storage Responsibility Summary:**
>
> - **MongoDB** handles **audit logs and change history only** — no primary business data.
> - **Azure Blob Storage** handles **all physical media file storage**; `MediaAsset.MediaUrl` stores the reference URL.
> - **SQL Server** handles all **structured business data**.

---

### 6.5 Key Design Notes

- **Soft deletes** — `ListingCase`, `MediaAsset`, and `User:IdentityUser` all use `IsDeleted` boolean flags instead of hard deletes. All queries must filter `IsDeleted = false` by default.
- **Hero image** — managed via `MediaAsset.IsHero` flag; only one MediaAsset per ListingCase should have `IsHero = true` at a time.
- **Selected media** — managed via `MediaAsset.IsSelect` flag; Agent toggles this to mark display images (max 10).
- **ListcaseStatus** currently has 3 values in the ER diagram (Created / Pending / Delivered). The workflow defined in Section 9 uses 4 stages — confirm with the team whether **In Review** should be added to the enum.
- **Agent is linked to ListingCase** via the `AgentListingCase` join table; a listing can have multiple agents assigned.

---

## 7. Feature Modules

### 7.1 Authentication & Account Management

- Admin accounts are initialised via Data Seeding when the system starts.
- Agent accounts are created by Admin; the system auto-generates a random password and emails the credentials to the Agent.
- Successful login returns a JWT Token; all endpoints require a valid token.
- Failed login attempts are logged to MongoDB UserActivityLog.

### 7.2 Listing Case Management

**Create Case (Admin Only)**

Required form fields:

- Address
- Sale type: For Sale / For Rent / Auction
- Property type: House / Unit / Townhouse / Villa / Others
- Bedrooms, Bathrooms, Garage, Area

**Listing Case List Page**

- Admin sees only listings they created.
- Agent sees only listings assigned to them.
- Supports filtering by status (Created / Pending / In Review / Delivered).
- Supports pagination.
- Each row shows: address, service type icon, current status, amount, Edit button, Preview button.

### 7.3 Media Management Module

**Admin — Upload Media**

Supported media types:

- Photos: jpg, png — multiple files allowed per upload
- Videography: mp4, mov — one file per upload
- Floor Plan: PDF — one file per upload
- VR Tour: gltf / vr files — one file per upload

Uploaded files are stored in Azure Blob Storage; the URL is written to the MediaAssets table.

**Multi-Quality Image Processing**

Each uploaded image automatically generates three versions:

- Print Quality (original high-resolution)
- Web Quality (compressed)
- Web Quality with Watermark (compressed + watermark)

**Agent — Select Display Media**

- Agent selects final display images from Admin-uploaded media (maximum 10 images).
- Selection is stored in the SelectedMedia table.
- Admin can view the Agent's final selection result.

### 7.4 Edit Mode (Shared by Admin & Agent)

Visual editing of the following modules:

- Property Description
- Photography section: select which images to display
- Floor Plan section
- Video section
- Location map
- Agent Contact section (name, phone, email, avatar, company name)

Edit features:

- Rich text / image editing
- Drag-and-drop to set Hero Image and Display Images
- Clicking an empty module navigates directly to its edit view
- All changes are saveable

### 7.5 Preview & Share

- Displays the final property showcase page in read-only mode.
- Shareable URL can be copied for client distribution.
- One-click download of all resources as a ZIP archive.

### 7.6 Resource Download

Each image supports download by quality type:

- Print Quality
- Web Quality
- Web Quality with Watermark

Videos and floor plans support direct download. All resources can be packaged into a single ZIP download.

### 7.7 Status Management (Admin Only)

See Section 9 for status transition rules. Admin can update listing status directly from the listing list page.

---

## 8. API Endpoints Overview

### Authentication

| Method | Path               | Auth   | Description                     |
| ------ | ------------------ | ------ | ------------------------------- |
| POST   | /auth/login        | Public | Login, returns JWT token        |
| POST   | /auth/register     | Admin  | Create an Agent account         |
| GET    | /users/me          | All    | Get current logged-in user info |
| PUT    | /users/me/password | All    | Update current user password    |

### Agent Management

| Method | Path                                  | Auth  | Description                             |
| ------ | ------------------------------------- | ----- | --------------------------------------- |
| GET    | /users                                | Admin | Get all users list (paginated)          |
| POST   | /agents                               | Admin | Create Agent and send credentials email |
| GET    | /agents                               | Admin | Get Agent list under current Admin      |
| GET    | /agents/search?email=                 | Admin | Exact-match Agent search by email       |
| POST   | /agents/{agentId}/photography-company | Admin | Link Agent to PhotographyCompany        |

### Listing Case

| Method | Path                        | Auth        | Description                                  |
| ------ | --------------------------- | ----------- | -------------------------------------------- |
| POST   | /listings                   | Admin       | Create Listing Case                          |
| GET    | /listings                   | Admin/Agent | Get Listing Case list (paginated + filtered) |
| GET    | /listings/{id}              | Admin/Agent | Get Listing Case details                     |
| PUT    | /listings/{id}              | Admin       | Update Listing Case info                     |
| DELETE | /listings/{id}              | Admin       | Delete Listing Case (cascading)              |
| PATCH  | /listings/{id}/status       | Admin       | Update Listing Case status                   |
| POST   | /listings/{id}/assign-agent | Admin       | Assign Agent to Listing Case                 |
| POST   | /listings/{id}/publish      | Admin/Agent | Generate shareable link                      |

### Media Management

| Method | Path                       | Auth        | Description                                    |
| ------ | -------------------------- | ----------- | ---------------------------------------------- |
| POST   | /listings/{id}/media       | Admin       | Upload media files (photo/video/floor plan/VR) |
| GET    | /listings/{id}/media       | Admin/Agent | Get all media for listing (grouped by type)    |
| DELETE | /media/{id}                | Admin       | Delete a media file                            |
| PUT    | /listings/{id}/cover-image | Admin/Agent | Set cover image (Hero Image)                   |

### Agent Display Selection

| Method | Path                           | Auth        | Description                           |
| ------ | ------------------------------ | ----------- | ------------------------------------- |
| PUT    | /listings/{id}/selected-media  | Agent       | Agent selects display media (max 10)  |
| GET    | /listings/{id}/final-selection | Admin/Agent | Get finalised display media selection |

### Preview & Download

| Method | Path                    | Auth        | Description                  |
| ------ | ----------------------- | ----------- | ---------------------------- |
| GET    | /listings/{id}/preview  | Admin/Agent | Get listing preview data     |
| GET    | /listings/{id}/download | Admin/Agent | ZIP download all media files |
| GET    | /media/{id}/download    | Admin/Agent | Download a single media file |

### Contacts

| Method | Path                    | Auth        | Description              |
| ------ | ----------------------- | ----------- | ------------------------ |
| POST   | /listings/{id}/contacts | Agent       | Add a contact to listing |
| GET    | /listings/{id}/contacts | Admin/Agent | Get listing contact list |

---

## 9. Status Transition Rules

```
Created → Pending → In Review → Delivered
```

| Status    | Description                                    | Trigger Condition                       |
| --------- | ---------------------------------------------- | --------------------------------------- |
| Created   | Listing created, media not yet uploaded        | Admin creates the listing               |
| Pending   | Media uploaded, awaiting Agent selection       | Admin uploads media and advances status |
| In Review | Agent has completed selection, Admin reviewing | Agent completes media selection         |
| Delivered | Page content finalised, delivery complete      | Admin confirms and closes               |

**Constraints:**

- Status can only flow **forward**; rollback is not permitted.
- Every status change is logged to MongoDB CaseHistory with operator ID and timestamp.
- Skip-level transitions (e.g. Created directly to Delivered) are rejected with `400 Bad Request`.

---

## 10. Task List

---

### Epic 1: Project Initialisation

#### Story 1.1: Project Setup

| #     | Task                                                           | Description                                                                                                                                                |
| ----- | -------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.1.1 | Initialise Git repository and create .gitignore                | Include rules for .NET, IDE files, and environment variables                                                                                               |
| 1.1.2 | Create solution and all project layers                         | Create `Remp.Solution` with `Remp.API`, `Remp.Service`, `Remp.Repository`, `Remp.DataAccess`, `Remp.Models`, `Remp.Common`; set correct project references |
| 1.1.3 | Create ASP.NET Core Web API entry project (`Remp.API`)         | .NET 8; configure Controllers, Middlewares, Program.cs, appsettings.json                                                                                   |
| 1.1.4 | Configure appsettings.json and environment variable management | Separate Development / Production configs                                                                                                                  |
| 1.1.5 | Configure Swagger / OpenAPI                                    | Include JWT authorisation header support                                                                                                                   |
| 1.1.6 | Configure Docker + docker-compose                              | API, SQL Server, and MongoDB as three services                                                                                                             |

#### Story 1.2: Dependencies & Base Services

| #     | Task                                                      | Description                                                                                                   |
| ----- | --------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| 1.2.1 | Configure Entity Framework Core (SQL Server)              | Install EF Core packages, configure DbContext                                                                 |
| 1.2.2 | Configure MongoDB Driver                                  | Configure connection string and Repository wrapper                                                            |
| 1.2.3 | Configure AutoMapper                                      | Register Profiles for Entity ↔ DTO mapping                                                                    |
| 1.2.4 | Configure FluentValidation                                | Add validation rules for all request DTOs; register via `AddValidatorsFromAssemblyContaining<T>()`            |
| 1.2.5 | Configure JWT authentication (ASP.NET Identity)           | Include token generation and parsing middleware                                                               |
| 1.2.6 | Create Global Exception Handler middleware                | Centralised exception handling, return standardised error responses; map exception types to HTTP status codes |
| 1.2.7 | Create generic ApiResponse class                          | Unified API response format (statusCode, message, errors, data)                                               |
| 1.2.8 | Create reusable Email Sender Service                      | Accepts title, body, and receiver parameters                                                                  |
| 1.2.9 | Enable XML documentation and configure Swagger to read it | Set `GenerateDocumentationFile=true` in .csproj; configure `IncludeXmlComments` in Swagger setup              |

---

### Epic 2: Database Design

#### Story 2.1: SQL Server Schema

| #      | Task                                                     | Description                                                              |
| ------ | -------------------------------------------------------- | ------------------------------------------------------------------------ |
| 2.1.1  | Design User / Role tables                                | Based on ASP.NET Identity, includes PhotographyCompany / Agent roles     |
| 2.1.2  | Design ListingCase table                                 | All business fields as defined in Section 6.2                            |
| 2.1.3  | Design MediaAsset table                                  | Includes BlobUrl, MediaType, ListingCaseId, etc.                         |
| 2.1.4  | Design SelectedMedia table                               | Stores Agent's final display media selections                            |
| 2.1.5  | Design CaseContact table                                 | Contact person information added by Agent                                |
| 2.1.6  | Design AgentPhotography join table                       | Agent ↔ PhotographyCompany association                                   |
| 2.1.7  | Design ListingAgents join table                          | Listing Case ↔ Agent assignment association                              |
| 2.1.8  | Design StatusHistory table                               | Listing status transition audit records                                  |
| 2.1.9  | Write EF Core Migration                                  | Run `dotnet ef migrations add Initial` and verify table structure        |
| 2.1.10 | Data Seeding: initialise roles and default Admin account | Insert PhotographyCompany / Agent roles and create default administrator |

#### Story 2.2: MongoDB Design

| #     | Task                                     | Description                                                         |
| ----- | ---------------------------------------- | ------------------------------------------------------------------- |
| 2.2.1 | Design CaseHistory collection            | Records listing case field changes and status transitions           |
| 2.2.2 | Design UserActivityLog collection        | Records login, register, upload, download, and other operation logs |
| 2.2.3 | Configure MongoDB connection and indexes | Create indexes on commonly queried fields                           |
| 2.2.4 | Implement MongoDB Repository wrapper     | Generic insert, delete, and query encapsulation                     |

---

### Epic 3: Authentication & User Management API

#### Story 3.1: Authentication API

| #     | Task                                                   | Description                                                                           |
| ----- | ------------------------------------------------------ | ------------------------------------------------------------------------------------- |
| 3.1.1 | Implement Login API (POST /auth/login)                 | Validate email/password, generate JWT, distinguish Admin / Agent role, log to MongoDB |
| 3.1.2 | Implement Get Current User API (GET /users/me)         | Parse JWT, return user ID, role, and assigned Listing IDs                             |
| 3.1.3 | Implement Update Password API (PUT /users/me/password) | Validate old password, update new password, log activity to MongoDB                   |

#### Story 3.2: User & Agent Management API

| #     | Task                                                                               | Description                                                                       |
| ----- | ---------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| 3.2.1 | Implement Get All Users API (GET /users)                                           | Admin Only, supports pagination                                                   |
| 3.2.2 | Implement Create Agent API (POST /agents)                                          | Admin Only, auto-generate random password, send credentials email, log to MongoDB |
| 3.2.3 | Implement Get Agent List (GET /agents)                                             | Admin Only, returns agents under current Admin account                            |
| 3.2.4 | Implement Search Agent by Email (GET /agents/search?email=)                        | Exact match; returns 404 if not found                                             |
| 3.2.5 | Implement Link Agent to PhotographyCompany (POST /agents/{id}/photography-company) | Returns 400 Bad Request if association already exists                             |

---

### Epic 4: Listing Case API

#### Story 4.1: Listing Case CRUD

| #     | Task                                                            | Description                                                                                  |
| ----- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| 4.1.1 | Create Listing Case API (POST /listings)                        | Admin Only, initial status is Created, log to MongoDB CaseHistory                            |
| 4.1.2 | Get Listing Case List (GET /listings)                           | Admin sees own listings; Agent sees assigned listings; supports status filter and pagination |
| 4.1.3 | Get Listing Case Details (GET /listings/{id})                   | Returns basic info, media list, status, and associated Agent info                            |
| 4.1.4 | Update Listing Case (PUT /listings/{id})                        | Admin Only, input validation, log changes to MongoDB                                         |
| 4.1.5 | Delete Listing Case (DELETE /listings/{id})                     | Admin Only, cascade delete media and contacts, log to MongoDB                                |
| 4.1.6 | Update Listing Status (PATCH /listings/{id}/status)             | Admin Only, enforce Created→Pending→In Review→Delivered, log to MongoDB                      |
| 4.1.7 | Assign Agent to Listing Case (POST /listings/{id}/assign-agent) | Admin Only, write to ListingAgents join table                                                |

---

### Epic 5: Media Management API

#### Story 5.1: Media Upload & Storage

| #     | Task                                                          | Description                                                                                                   |
| ----- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| 5.1.1 | Configure Azure Blob Storage                                  | Set up container, access policy, and secure URL generation                                                    |
| 5.1.2 | Create reusable BlobUpload Service                            | Supports image, video, and VR file formats                                                                    |
| 5.1.3 | Create multi-quality image processing service                 | Auto-generate Print / Web / Web+Watermark versions after upload                                               |
| 5.1.4 | Implement Upload Media Assets API (POST /listings/{id}/media) | Admin Only; photos allow multi-file; video/floor plan/VR allow one file per upload; write to MediaAsset table |

#### Story 5.2: Media Query & Deletion

| #     | Task                                              | Description                                                           |
| ----- | ------------------------------------------------- | --------------------------------------------------------------------- |
| 5.2.1 | Get Listing Media List (GET /listings/{id}/media) | Admin / Agent, response grouped by MediaType                          |
| 5.2.2 | Delete Media File (DELETE /media/{id})            | Admin Only, verify ownership, delete from Blob and DB, log to MongoDB |
| 5.2.3 | Set Cover Image (PUT /listings/{id}/cover-image)  | Admin / Agent, verify mediaId belongs to the listing                  |

#### Story 5.3: Media Download

| #     | Task                                                  | Description                                                      |
| ----- | ----------------------------------------------------- | ---------------------------------------------------------------- |
| 5.3.1 | Create reusable BlobDownload Service                  | Returns (Stream, ContentType, FileName)                          |
| 5.3.2 | Download Single Media File (GET /media/{id}/download) | Supports quality type parameter (print / web / watermark)        |
| 5.3.3 | ZIP Download All Media (GET /listings/{id}/download)  | Fetch all files from Blob, compress into ZIP, return as download |

---

### Epic 6: Agent Display Selection API

| #   | Task                                                             | Description                                                                                       |
| --- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| 6.1 | Agent Select Display Media (PUT /listings/{id}/selected-media)   | Agent Only, max 10 images, validate media ownership, store in SelectedMedia table, log to MongoDB |
| 6.2 | Get Final Display Selection (GET /listings/{id}/final-selection) | Admin / Agent, aggregate and return from SelectedMedia table                                      |
| 6.3 | Add Case Contact (POST /listings/{id}/contacts)                  | Agent Only, add contact info to CaseContact table                                                 |
| 6.4 | Get Case Contact List (GET /listings/{id}/contacts)              | Admin / Agent                                                                                     |

---

### Epic 7: Preview & Share API

| #   | Task                                                  | Description                                                         |
| --- | ----------------------------------------------------- | ------------------------------------------------------------------- |
| 7.1 | Get Preview Data (GET /listings/{id}/preview)         | Admin / Agent, returns full data needed to render the showcase page |
| 7.2 | Generate Shareable Link (POST /listings/{id}/publish) | Generate unique URL, write to ListingCase.ShareableUrl field        |

---

### Epic 8: Code Quality & Refactoring

| #   | Task                                                      | Description                                                                                                   |
| --- | --------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| 8.1 | Global code refactoring to follow SOLID principles        | Review interfaces and implementations, eliminate duplicate code                                               |
| 8.2 | Enforce FluentValidation across all request DTOs          | Audit every endpoint to confirm a validator exists; cover required fields, length, enum, range, email format  |
| 8.3 | Audit and complete Exception Middleware coverage          | Verify all exception types map to correct HTTP codes; confirm Production hides stack trace                    |
| 8.4 | Audit all multi-table operations for transaction wrapping | Verify every operation listed in Section 3.3 uses `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`   |
| 8.5 | Audit DI registrations and service lifetimes              | Confirm all services use interface-based registration with correct lifetimes (Scoped / Singleton / Transient) |
| 8.6 | Audit XML comments on all controller actions              | Ensure every action has `<summary>`, `<param>`, and `<response>` tags; verify they appear in Swagger UI       |
| 8.7 | Write README documentation                                | Project overview, local setup steps, environment variable configuration                                       |
| 8.8 | Write database schema documentation                       | ER diagram + relationship table descriptions                                                                  |

---

### Epic 9: Unit Testing

#### Story 9.1: Core Business Logic Tests

| #     | Task                                                  | Description                                                               |
| ----- | ----------------------------------------------------- | ------------------------------------------------------------------------- |
| 9.1.1 | Listing Case API unit tests (xUnit)                   | Cover create, update, delete, status change with valid and invalid inputs |
| 9.1.2 | Validate status transition logic                      | Assert all illegal transitions (rollback, skip-level) return 400          |
| 9.1.3 | Media Management API unit tests                       | Cover upload, query, delete with file type and permission checks          |
| 9.1.4 | Agent SelectMedia and UploadMedia unit tests          | Validate max 10 image limit and media ownership verification              |
| 9.1.5 | AddAgentToListingCase service unit tests              | Validate assignment logic and duplicate assignment handling               |
| 9.1.6 | UpdateListingCase and UpdateStatus service unit tests | Validate business rules and MongoDB log writes                            |

#### Story 9.2: Dependency Mocking

| #     | Task                                            | Description                                        |
| ----- | ----------------------------------------------- | -------------------------------------------------- |
| 9.2.1 | Use Moq to mock DB, Blob, and auth dependencies | Isolate unit tests from live services              |
| 9.2.2 | End-to-end API tests (Postman / Newman)         | Integration tests covering main business workflows |

---

### Epic 10: Deployment

#### Story 10.1: Azure Cloud Deployment

| #      | Task                                                     | Description                                                          |
| ------ | -------------------------------------------------------- | -------------------------------------------------------------------- |
| 10.1.1 | Write Dockerfile                                         | Multi-stage build to optimise image size                             |
| 10.1.2 | Configure Azure App Service                              | Host the API, configure environment variables                        |
| 10.1.3 | Configure Azure SQL Database                             | Production database, run EF Core migrations                          |
| 10.1.4 | Configure Azure Blob Storage                             | Create media container, configure CORS and access policy             |
| 10.1.5 | Configure MongoDB (Atlas or Azure Cosmos DB for MongoDB) | Audit log database — confirm final selection and unify documentation |
| 10.1.6 | Configure CI/CD (GitHub Actions)                         | Pipeline: automated tests → build → deploy                           |
| 10.1.7 | Write CI/CD deployment guide                             | Environment variable setup, secrets management                       |

---

_End of Document — Please update the version number and note any changes when requirements are modified._
