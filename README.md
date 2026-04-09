# 📚 FirstApi — ASP.NET Core Books REST API with JWT Authentication

A RESTful Web API built with **ASP.NET Core (.NET 10)** that provides full CRUD operations for managing a collection of books, secured with **JWT (JSON Web Token) authentication**. The API uses **Entity Framework Core** with **PostgreSQL** for data persistence and includes **OpenAPI/Swagger** documentation.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Configure the Database & JWT](#2-configure-the-database--jwt)
  - [3. Apply Migrations](#3-apply-migrations)
  - [4. Run the Application](#4-run-the-application)
- [Testing](#testing)
- [API Endpoints](#api-endpoints)
  - [Authentication API](#authentication-api)
  - [Books API (Protected)](#books-api-protected)
- [Authentication Flow](#authentication-flow)
- [Request & Response Examples](#request--response-examples)
- [Password Requirements](#password-requirements)
- [Configuration](#configuration)
- [Potential Improvements](#potential-improvements)
- [License](#license)

---

## Features

- **Rate Limiting** — Fixed window rate limiter configured to protect Authentication endpoints from brute-force attacks.
- **JWT Authentication** — Secure register/login endpoints with BCrypt password hashing and JWT token generation.
- **Refresh Tokens** — Long-lived refresh tokens with rotation for seamless token renewal without re-authentication.
- **Email Verification** — OTP-based email verification on registration via SMTP (MailKit). Emails are sent fire-and-forget with error logging on failure.
- **Password Reset** — Forgot password and reset password flow with time-limited OTP tokens.
- **Protected Routes** — Books API requires a valid Bearer token to access.
- **Ownership-Based Access** — Users can only view, edit, and delete books they created.
- **Full CRUD API** — Create, Read, Update, and Delete book records.
- **Service/Repository Pattern** — Clean layered architecture separating HTTP handling (controllers), business logic (services), and data access (repositories). All layers communicate through interfaces for testability and loose coupling.
- **Input Validation** — Data annotations (`[Required]`, `[EmailAddress]`, `[MinLength]`) on all request DTOs enforce validation automatically before reaching the service layer.
- **Standardized Responses** — All endpoints return a consistent `BaseResponse<T>` wrapper with `success`, `message`, and `data` fields.
- **Global Error Handling** — Centralized middleware maps exceptions to HTTP status codes, removing repetitive try/catch blocks across controllers.
- **Structured Logging** — `ILogger<T>` used centrally in error handling and services for structured error and info logging.
- **Strongly-Typed Configuration** — JWT and email settings are bound to typed options classes (`JwtOptions`, `EmailVerificationOptions`) instead of raw string parsing.
- **Entity Framework Core** — Code-first approach with migrations for database schema management.
- **PostgreSQL** — Configured to use PostgreSQL as the relational database. FK relationship between `Book` and `User` with cascade delete.
- **OpenAPI / Swagger** — Auto-generated API documentation available in development mode.
- **Async Operations** — All database operations are fully asynchronous.
- **Automated Testing** — Unit tests (xUnit + Moq) for service-layer business logic and integration tests using `WebApplicationFactory` with an in-memory database.
- **CI/CD** — GitHub Actions workflow runs build and tests on every push to `develop` and pull request to `main`.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET SDK | 10.0 | Runtime & framework |
| ASP.NET Core | 10.0 | Web API framework |
| Entity Framework Core | 10.0.5 | ORM / data access |
| Npgsql (EF Core Provider) | 10.0.1 | PostgreSQL database driver |
| PostgreSQL | 15+ | Relational database |
| BCrypt.Net-Next | 4.1.0 | Password hashing |
| MailKit | 4.15.1 | Email sending (SMTP) |
| JWT Bearer Authentication | 10.0.5 | Token-based authentication |
| OpenAPI | 10.0.3 | API documentation |
| xUnit | 2.9.3 | Testing framework |
| Moq | 4.20.72 | Mocking library for unit tests |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.5 | Integration test host |
| EF Core InMemory | 10.0.5 | In-memory database for testing |

---

## Project Structure

```
FirstApi/
├── Controllers/
│   ├── AuthController.cs              # Auth endpoints (register, login, verify, reset, refresh, logout)
│   └── BooksController.cs             # Books CRUD endpoints (protected, ownership-based)
├── Data/
│   └── FirstApiContext.cs             # EF Core DbContext with FK config
├── DTOs/
│   ├── AuthResponse.cs                # Login response (token, refresh token, user info, expiration)
│   ├── BaseResponse.cs                # Generic API response wrapper
│   ├── CreateBookRequest.cs           # Create/update book request body (title, author, yearPublished)
│   ├── ForgetPasswordRequest.cs       # Forgot password request body
│   ├── LoginRequest.cs                # Login request body
│   ├── RefreshTokenRequest.cs         # Refresh token request body
│   ├── RegisterRequest.cs             # Registration request body
│   ├── ResetPasswordRequest.cs        # Reset password request body (email, token, password)
│   ├── UserDto.cs                     # User data without sensitive fields
│   └── VerifyEmailRequest.cs          # Email verification request body (email, token)
├── Middleware/
│   └── ExceptionMiddleware.cs         # Catch-all error handler translating exceptions to HTTP responses
├── Models/
│   ├── Books.cs                       # Book entity (linked to User via UserId)
│   └── User.cs                        # User entity with verification, reset & refresh token fields
├── Options/
│   ├── JwtOptions.cs                  # Strongly-typed JWT configuration
│   └── EmailVerificationOptions.cs    # Strongly-typed email verification configuration
├── Repositories/
│   ├── Interfaces/
│   │   ├── IAuthRepository.cs         # Auth data access contract
│   │   └── IBookRepository.cs         # Book data access contract
│   ├── AuthRepository.cs              # Auth data access (EF Core queries for Users)
│   └── BookRepository.cs              # Book data access (EF Core queries for Books)
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs            # Auth business logic contract
│   │   ├── IBookService.cs            # Book business logic contract
│   │   └── IEmailService.cs           # Email service contract
│   ├── AuthService.cs                 # Auth logic (register, login, verify, reset, refresh, JWT)
│   ├── BookService.cs                 # Book logic (validation, ownership, orchestration)
│   └── EmailService.cs                # SMTP email sending via MailKit (fire-and-forget)
├── FirstApi.Tests/
│   ├── UnitTests/
│   │   ├── Services/
│   │   │   ├── AuthServiceTests.cs    # Unit tests for AuthService (Moq-based)
│   │   │   └── BookServiceTests.cs    # Unit tests for BookService (Moq-based)
│   │   └── Repositories/
│   │       └── BookRepositoryTests.cs # Unit tests for BookRepository (InMemory DB)
│   ├── IntegrationTests/
│   │   ├── CustomWebApplicationFactory.cs  # Test server factory (InMemory DB + config)
│   │   └── AuthControllerTests.cs     # Integration tests for Auth endpoints
│   └── FirstApi.Tests.csproj          # Test project file
├── .github/
│   └── workflows/
│       └── bookapi.yml                # CI workflow (build + test on push/PR)
├── Properties/
│   └── launchSettings.json            # Launch/debug profiles
├── appsettings.json                   # App configuration (DB, JWT, SMTP settings)
├── appsettings.Development.json       # Development-specific overrides
├── Program.cs                         # App entry point, middleware & DI configuration
├── FirstApi.csproj                    # Project file with NuGet dependencies
├── FirstApi.sln                       # Solution file
├── FirstApi.http                      # HTTP request samples for testing
└── README.md
```

### Architecture

```
Controller (HTTP) → Service (Business Logic) → Repository (Data Access) → Database
```

Each layer communicates through interfaces, enabling loose coupling and testability.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (v15 or later recommended)
- [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd FirstApi
```

### 2. Configure the Database & JWT

Create an `appsettings.json` file in the project root:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=namedb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Issuer": "https://localhost:7000",
    "Audience": "https://localhost:7000",
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 30
  },
  "EmailVerification": {
    "ExpirationInMinutes": 15
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "BookApi",
    "Password": "your-app-password"
  }
}
```

> ⚠️ **Note:** `appsettings.json` is excluded from version control. You must create this file locally with your own credentials and JWT secret key.

### 3. Apply Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

You can run the application using either the .NET CLI natively, or via Docker for a complete containerized environment.

#### Option A: Native (.NET)
```bash
dotnet run
```
The API will start on the URL configured in `Properties/launchSettings.json`.

#### Option B: Docker Compose (Recommended)
```bash
docker-compose up --build
```
Docker will automatically spin up a PostgreSQL instance, build the API container, apply any pending EF Core database migrations, and map the API to `http://localhost:5001`.

---

In development mode, OpenAPI docs are available at:
```
http://localhost:5001/openapi/v1.json
```

---

## Testing

The project includes both **unit tests** and **integration tests** using xUnit.

### Test Structure

| Type | Location | Description |
|---|---|---|
| **Unit Tests** | `FirstApi.Tests/UnitTests/Services/` | Test service-layer business logic with Moq-mocked dependencies |
| **Unit Tests** | `FirstApi.Tests/UnitTests/Repositories/` | Test repository-layer data access with EF Core InMemory database |
| **Integration Tests** | `FirstApi.Tests/IntegrationTests/` | Test full HTTP request pipeline using `WebApplicationFactory` |

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run only unit tests
dotnet test --filter "UnitTests"

# Run only integration tests
dotnet test --filter "IntegrationTests"
```

### Integration Test Setup

Integration tests use a `CustomWebApplicationFactory` that:
- Replaces the PostgreSQL database with an **EF Core InMemory** database
- Provides test JWT and email verification configuration
- Uses a unique database name per test run to prevent cross-contamination
- Sets the environment to `Testing`

---

## API Endpoints

### Authentication API

Base URL: `/api/Auth`

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/Auth/register` | Register a new user (sends verification OTP) | ❌ No |
| `POST` | `/api/Auth/login` | Login and receive JWT + refresh token | ❌ No |
| `POST` | `/api/Auth/refresh-token` | Exchange refresh token for new JWT + refresh token | ❌ No |
| `POST` | `/api/Auth/verify-email` | Verify email with OTP code | ❌ No |
| `POST` | `/api/Auth/resend-email-verification-token` | Resend verification OTP | ❌ No |
| `POST` | `/api/Auth/forgot-password` | Request a password reset OTP | ❌ No |
| `POST` | `/api/Auth/reset-password` | Reset password with OTP code | ❌ No |
| `POST` | `/api/Auth/logout` | Revoke refresh token (invalidates session) | ❌ No |

### Books API (Protected — Ownership-Based)

Base URL: `/api/Books`

> 🔒 All Books endpoints require a valid JWT token. Users can only access books they created.

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/api/Books` | Retrieve all books owned by the logged-in user | ✅ Yes |
| `GET` | `/api/Books/{id}` | Retrieve a single book by ID (must be owner) | ✅ Yes |
| `POST` | `/api/Books` | Create a new book (auto-linked to logged-in user) | ✅ Yes |
| `PUT` | `/api/Books/{id}` | Update a book (must be owner) | ✅ Yes |
| `DELETE` | `/api/Books/{id}` | Delete a book (must be owner) | ✅ Yes |

---

## Authentication Flow

```
1. Register → POST /api/Auth/register
   └── Returns user info (201 Created) + sends verification OTP to email

2. Verify Email → POST /api/Auth/verify-email
   └── Submit OTP from email to verify account

3. Login → POST /api/Auth/login
   └── Returns JWT token (60 min) + refresh token (30 days)

4. Access protected routes → GET /api/Books
   └── Include header: Authorization: Bearer <your-token>

5. Token expires → POST /api/Auth/refresh-token
   └── Send refresh token → get new JWT + new refresh token (rotation)

6. Logout → POST /api/Auth/logout
   └── Send refresh token → revokes it server-side (invalidates session)
```

### Password Reset Flow

```
1. Forgot Password → POST /api/Auth/forgot-password
   └── Sends reset OTP to email (always returns 200)

2. Reset Password → POST /api/Auth/reset-password
   └── Submit OTP + new password to reset
```

---

## Request & Response Examples

### Register a User

```http
POST /api/Auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response** `201 Created`:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "isEmailVerified": false,
    "createdAt": "2026-04-03T23:00:00Z",
    "updatedAt": "2026-04-03T23:00:00Z"
  }
}
```

> A 4-digit OTP is sent to the user's email for verification.

### Verify Email

```http
POST /api/Auth/verify-email
Content-Type: application/json

{
  "email": "john@example.com",
  "token": "7598"
}
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "Email verified successfully",
  "data": true
}
```

### Login

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "User logged in successfully",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a8Kx9mN2pQ7...",
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "isEmailVerified": true,
    "createdAt": "2026-04-03T23:00:00Z",
    "updatedAt": "2026-04-03T23:00:00Z",
    "tokenExpiration": "2026-04-03T23:15:00Z"
  }
}
```

### Refresh Token

```http
POST /api/Auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "a8Kx9mN2pQ7..."
}
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "token": "eyJnZXdKb1...",
    "refreshToken": "b7Zy3kP4...",
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "isEmailVerified": true,
    "createdAt": "2026-04-03T23:00:00Z",
    "updatedAt": "2026-04-03T23:00:00Z",
    "tokenExpiration": "2026-04-04T00:15:00Z"
  }
}
```

> **Note:** Refresh token rotation — each refresh invalidates the previous refresh token and issues a new one.

### Forgot Password

```http
POST /api/Auth/forgot-password
Content-Type: application/json

{
  "email": "john@example.com"
}
```

**Response** `200 OK` (always, regardless of whether email exists):
```json
{
  "success": true,
  "message": "Password reset token sent successfully",
  "data": true
}
```

### Reset Password

```http
POST /api/Auth/reset-password
Content-Type: application/json

{
  "email": "john@example.com",
  "token": "4821",
  "password": "NewPassword123!"
}
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "Password reset successfully",
  "data": true
}
```

### Logout

```http
POST /api/Auth/logout
Content-Type: application/json

{
  "refreshToken": "a8Kx9mN2pQ7..."
}
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "Logged out successfully",
  "data": true
}
```

### Get All Books (owned by logged-in user)

```http
GET /api/Books
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "Books fetched successfully",
  "data": [
    {
      "id": 5,
      "userId": 1,
      "title": "The Great Gatsby",
      "author": "F. Scott Fitzgerald",
      "yearPublished": 1925
    }
  ]
}
```

**Without token** → `401 Unauthorized`

### Create a Book

```http
POST /api/Books
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "title": "Brave New World",
  "author": "Aldous Huxley",
  "yearPublished": 1932
}
```

**Response** `201 Created`:
```json
{
  "success": true,
  "message": "Book created successfully",
  "data": {
    "id": 6,
    "userId": 1,
    "title": "Brave New World",
    "author": "Aldous Huxley",
    "yearPublished": 1932
  }
}
```

> **Note:** `userId` is automatically set from your JWT token — you don't need to include it in the request body.

### Delete a Book

```http
DELETE /api/Books/6
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** `204 No Content`

### Error Response Example

```json
{
  "success": false,
  "message": "Book not found",
  "data": null
}
```

---

## Password Requirements

Passwords must meet all of the following criteria:

| Requirement | Rule |
|---|---|
| Minimum length | At least 6 characters |
| Uppercase | At least one uppercase letter |
| Lowercase | At least one lowercase letter |
| Digit | At least one number |
| Special character | At least one non-alphanumeric character (e.g., `!@#$%`) |

---

## Configuration

| Setting | Location | Description |
|---|---|---|
| Connection String | `appsettings.json` → `ConnectionStrings.DefaultConnection` | PostgreSQL connection details |
| JWT Secret Key | `appsettings.json` → `Jwt.Key` | Key used to sign JWT tokens (min 32 chars) |
| JWT Issuer | `appsettings.json` → `Jwt.Issuer` | Token issuer for validation |
| JWT Audience | `appsettings.json` → `Jwt.Audience` | Token audience for validation |
| Token Expiry | `appsettings.json` → `Jwt.ExpirationInMinutes` | Access token lifetime in minutes (default: 60) |
| Refresh Token Expiry | `appsettings.json` → `Jwt.RefreshTokenExpirationInDays` | Refresh token lifetime in days (default: 30) |
| SMTP Server | `appsettings.json` → `EmailSettings.SmtpServer` | Email server (e.g., `smtp.gmail.com`) |
| SMTP Port | `appsettings.json` → `EmailSettings.SmtpPort` | SMTP port (587 for TLS) |
| Sender Email | `appsettings.json` → `EmailSettings.SenderEmail` | Email address to send from |
| Sender Password | `appsettings.json` → `EmailSettings.Password` | App password for SMTP auth |
| OTP Expiry | `appsettings.json` → `EmailVerification.ExpirationInMinutes` | OTP token lifetime in minutes |
| Logging | `appsettings.json` → `Logging` | Log level configuration |

---

## Potential Improvements

- [x] Add **email verification** on registration
- [x] Implement **password reset** flow
- [x] Add **refresh tokens** for seamless token renewal
- [x] Add a **Service/Repository layer** to separate concerns
- [x] Write **unit and integration tests** with xUnit
- [x] Add **Docker support** with `docker-compose.yml`
- [x] Implement **global error handling middleware**
- [x] Add **rate limiting** to prevent brute-force attacks
- [x] Set up **CI/CD** with GitHub Actions
- [ ] Add **API versioning** (to be done later)
- [ ] Refactor to **Clean Architecture** — introduce a proper Domain layer and separate Application, Infrastructure concerns; decouple business logic from data access (to be done later)
- [ ] Add **CQRS with MediatR** — separate read (queries) from write (commands) operations for cleaner, testable handlers (to be done later)
- [x] Replace manual validation with **FluentValidation** — cleaner, reusable validation rules that integrate with the DI container
- [ ] Add **caching** — implement IMemoryCache on frequently read endpoints (e.g. GET /api/Books) and Redis for multi-instance support (to be done later)
- [x] Add **Hangfire** for proper background job processing — move fire-and-forget email sending to a persistent, retriable job queue
- [x] Add **ownership-based filtering at the repository level** — move the userId filter into BookRepository instead of BookService for cleaner separation
- [x] Implement **Problem Details (RFC 7807)** — standardize error responses to the RFC format instead of the custom BaseResponse wrapper for errors
- [ ] Add **explicit LINQ queries** in repositories — replace implicit EF Core queries with explicit Where/Select/OrderBy chains for better readability and control
- [ ] Add **refresh token family tracking** — detect refresh token reuse attacks by invalidating an entire token family on suspicious reuse
- [ ] Add **structured logging with Serilog** — replace ILogger with Serilog for structured, queryable logs with sinks (file, Seq, Application Insights)
- [ ] Write **tests for BookService** — unit tests for business logic (ownership checks, not-found scenarios) are missing alongside the existing AuthService tests

---

## License

This project is open source and available for learning purposes.
