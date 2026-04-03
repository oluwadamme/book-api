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
- [API Endpoints](#api-endpoints)
  - [Authentication API](#authentication-api)
  - [Books API (Protected)](#books-api-protected)
- [Authentication Flow](#authentication-flow)
- [Request & Response Examples](#request--response-examples)
- [Password Requirements](#password-requirements)
- [Seed Data](#seed-data)
- [Configuration](#configuration)
- [Potential Improvements](#potential-improvements)
- [License](#license)

---

## Features

- **JWT Authentication** — Secure register/login endpoints with BCrypt password hashing and JWT token generation.
- **Protected Routes** — Books API requires a valid Bearer token to access.
- **Full CRUD API** — Create, Read, Update, and Delete book records.
- **Input Validation** — Email and password validation with strong password requirements.
- **Standardized Responses** — All endpoints return a consistent `BaseResponse<T>` wrapper with `success`, `message`, and `data` fields.
- **Entity Framework Core** — Code-first approach with migrations for database schema management.
- **PostgreSQL** — Configured to use PostgreSQL as the relational database.
- **Seed Data** — Automatically populates the database with sample books on initial migration.
- **OpenAPI / Swagger** — Auto-generated API documentation available in development mode.
- **Async Operations** — All database operations are fully asynchronous.

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
| JWT Bearer Authentication | 10.0.5 | Token-based authentication |
| OpenAPI | 10.0.3 | API documentation |

---

## Project Structure

```
FirstApi/
├── Controllers/
│   ├── AuthController.cs        # Authentication endpoints (register, login)
│   └── BooksController.cs       # Books CRUD endpoints (protected)
├── Data/
│   └── FirstApiContext.cs       # EF Core DbContext with seed data & User config
├── DTOs/
│   ├── AuthResponse.cs          # Login response (token, user info, expiration)
│   ├── BaseResponse.cs          # Generic API response wrapper
│   ├── LoginRequest.cs          # Login request body
│   ├── RegisterRequest.cs       # Registration request body
│   └── UserDto.cs               # User data without sensitive fields
├── Models/
│   ├── Books.cs                 # Book entity
│   └── User.cs                  # User entity (Id, name, email, passwordHash)
├── Services/
│   └── AuthService.cs           # Auth business logic (register, login, JWT generation)
├── Properties/
│   └── launchSettings.json      # Launch/debug profiles
├── appsettings.json             # App configuration (DB, JWT settings)
├── appsettings.Development.json # Development-specific overrides
├── Program.cs                   # App entry point, middleware & DI configuration
├── FirstApi.csproj              # Project file with NuGet dependencies
├── FirstApi.sln                 # Solution file
├── FirstApi.http                # HTTP request samples for testing
└── README.md
```

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
    "ExpirationInMinutes": 60
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

```bash
dotnet run
```

The API will start on the URL configured in `Properties/launchSettings.json`. In development mode, OpenAPI docs are available at:
```
https://localhost:<port>/openapi/v1.json
```

---

## API Endpoints

### Authentication API

Base URL: `/api/Auth`

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/Auth/register` | Register a new user account | ❌ No |
| `POST` | `/api/Auth/login` | Login and receive a JWT token | ❌ No |

### Books API (Protected)

Base URL: `/api/Books`

> 🔒 All Books endpoints require a valid JWT token in the `Authorization` header.

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/api/Books` | Retrieve all books | ✅ Yes |
| `GET` | `/api/Books/{id}` | Retrieve a single book by ID | ✅ Yes |
| `POST` | `/api/Books` | Create a new book | ✅ Yes |
| `PUT` | `/api/Books/{id}` | Update an existing book | ✅ Yes |
| `DELETE` | `/api/Books/{id}` | Delete a book | ✅ Yes |

---

## Authentication Flow

```
1. Register → POST /api/Auth/register
   └── Returns user info (no token)

2. Login → POST /api/Auth/login
   └── Returns JWT token + user info + expiration

3. Access protected routes → GET /api/Books
   └── Include header: Authorization: Bearer <your-token>
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

**Response** `200 OK`:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "createdAt": "2026-04-03T23:00:00Z",
    "updatedAt": "2026-04-03T23:00:00Z"
  }
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
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "createdAt": "2026-04-03T23:00:00Z",
    "updatedAt": "2026-04-03T23:00:00Z",
    "tokenExpiration": "2026-04-04T00:00:00Z"
  }
}
```

### Access Protected Route

```http
GET /api/Books
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** `200 OK`:
```json
[
  {
    "id": 1,
    "title": "The Great Gatsby",
    "author": "F. Scott Fitzgerald",
    "yearPublished": 1925
  }
]
```

**Without token** → `401 Unauthorized`

### Error Response Example

```json
{
  "success": false,
  "message": "User already exists",
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

## Seed Data

The database is pre-populated with the following books when migrations are applied:

| ID | Title | Author | Year Published |
|---|---|---|---|
| 1 | The Great Gatsby | F. Scott Fitzgerald | 1925 |
| 2 | To Kill a Mockingbird | Harper Lee | 1960 |
| 3 | 1984 | George Orwell | 1949 |

---

## Configuration

| Setting | Location | Description |
|---|---|---|
| Connection String | `appsettings.json` → `ConnectionStrings.DefaultConnection` | PostgreSQL connection details |
| JWT Secret Key | `appsettings.json` → `Jwt.Key` | Key used to sign JWT tokens (min 32 chars) |
| JWT Issuer | `appsettings.json` → `Jwt.Issuer` | Token issuer for validation |
| JWT Audience | `appsettings.json` → `Jwt.Audience` | Token audience for validation |
| Token Expiry | `appsettings.json` → `Jwt.ExpirationInMinutes` | Token lifetime in minutes |
| Logging | `appsettings.json` → `Logging` | Log level configuration |

---

## Potential Improvements

- [ ] Add **refresh tokens** for seamless token renewal
- [ ] Implement **role-based authorization** (Admin, User)
- [ ] Add **email verification** on registration
- [ ] Implement **password reset** flow
- [ ] Add a **Service/Repository layer** to separate concerns
- [ ] Write **unit and integration tests** with xUnit
- [ ] Add **Docker support** with `docker-compose.yml`
- [ ] Implement **global error handling middleware**
- [ ] Add **API versioning**
- [ ] Add **rate limiting** to prevent brute-force attacks
- [ ] Set up **CI/CD** with GitHub Actions

---

## License

This project is open source and available for learning purposes.