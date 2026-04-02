# 📚 FirstApi — ASP.NET Core Books REST API

A RESTful Web API built with **ASP.NET Core (.NET 10)** that provides full CRUD operations for managing a collection of books. The API uses **Entity Framework Core** with **PostgreSQL** for data persistence and includes **OpenAPI/Swagger** documentation out of the box.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Configure the Database](#2-configure-the-database)
  - [3. Apply Migrations](#3-apply-migrations)
  - [4. Run the Application](#4-run-the-application)
- [API Endpoints](#api-endpoints)
  - [Books API](#books-api)
- [Request & Response Examples](#request--response-examples)
- [Seed Data](#seed-data)
- [Configuration](#configuration)
- [Potential Improvements](#potential-improvements)
- [License](#license)

---

## Features

- **Full CRUD API** — Create, Read, Update, and Delete book records via RESTful endpoints.
- **Entity Framework Core** — Code-first approach with migrations for database schema management.
- **PostgreSQL** — Configured to use PostgreSQL as the relational database provider.
- **Seed Data** — Automatically populates the database with sample books on initial migration.
- **OpenAPI / Swagger** — Auto-generated interactive API documentation available in development mode.
- **Async Operations** — All database operations are fully asynchronous for better scalability.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET SDK | 10.0 | Runtime & framework |
| ASP.NET Core | 10.0 | Web API framework |
| Entity Framework Core | 10.0.5 | ORM / data access |
| Npgsql (EF Core Provider) | 10.0.1 | PostgreSQL database driver |
| PostgreSQL | 15+ | Relational database |
| OpenAPI | 10.0.3 | API documentation |

---

## Project Structure

```
FirstApi/
├── Controllers/
│   └── BooksController.cs      # REST API controller with CRUD endpoints
├── Data/
│   └── FirstApiContext.cs       # EF Core DbContext with seed data
├── Models/
│   └── Books.cs                 # Book entity model
├── Properties/
│   └── launchSettings.json      # Launch/debug profiles
├── appsettings.json             # App configuration (connection strings, logging)
├── appsettings.Development.json # Development-specific overrides
├── Program.cs                   # Application entry point & service configuration
├── FirstApi.csproj              # Project file with NuGet dependencies
├── FirstApi.sln                 # Solution file
└── README.md
```

---

## Prerequisites

Before running the project, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (v15 or later recommended)
- [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) — install globally with:
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

### 2. Configure the Database

Create an `appsettings.json` file in the project root with your PostgreSQL connection string:

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
    "DefaultConnection": "Server=localhost;Port=5432;Database=first_api_db;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

> ⚠️ **Note:** `appsettings.json` is excluded from version control. You must create this file locally with your own credentials.

### 3. Apply Migrations

Generate and apply the database schema:

```bash
# Create a new migration (if Migrations folder doesn't exist)
dotnet ef migrations add InitialCreate

# Apply migrations to the database
dotnet ef database update
```

This will create the `first_api_db` database, the `Books` table, and seed it with sample data.

### 4. Run the Application

```bash
dotnet run
```

The API will start on:
- **HTTPS:** `https://localhost:5001`
- **HTTP:** `http://localhost:5000`

> Check `Properties/launchSettings.json` for the exact ports configured for your environment.

In development mode, the OpenAPI documentation is available at:
```
https://localhost:5001/openapi/v1.json
```

---

## API Endpoints

### Books API

Base URL: `/api/Books`

| Method | Endpoint | Description | Success Code |
|---|---|---|---|
| `GET` | `/api/Books` | Retrieve all books | `200 OK` |
| `GET` | `/api/Books/{id}` | Retrieve a single book by ID | `200 OK` |
| `POST` | `/api/Books` | Create a new book | `201 Created` |
| `PUT` | `/api/Books/{id}` | Update an existing book | `204 No Content` |
| `DELETE` | `/api/Books/{id}` | Delete a book | `204 No Content` |

---

## Request & Response Examples

### Get All Books

```http
GET /api/Books
```

**Response** `200 OK`:
```json
[
  {
    "id": 1,
    "title": "The Great Gatsby",
    "author": "F. Scott Fitzgerald",
    "yearPublished": 1925
  },
  {
    "id": 2,
    "title": "To Kill a Mockingbird",
    "author": "Harper Lee",
    "yearPublished": 1960
  }
]
```

### Get a Single Book

```http
GET /api/Books/1
```

**Response** `200 OK`:
```json
{
  "id": 1,
  "title": "The Great Gatsby",
  "author": "F. Scott Fitzgerald",
  "yearPublished": 1925
}
```

**Response** `404 Not Found` — if the book doesn't exist.

### Create a Book

```http
POST /api/Books
Content-Type: application/json

{
  "title": "Brave New World",
  "author": "Aldous Huxley",
  "yearPublished": 1932
}
```

**Response** `201 Created`:
```json
{
  "id": 4,
  "title": "Brave New World",
  "author": "Aldous Huxley",
  "yearPublished": 1932
}
```

### Update a Book

```http
PUT /api/Books/1
Content-Type: application/json

{
  "title": "The Great Gatsby (Revised)",
  "author": "F. Scott Fitzgerald",
  "yearPublished": 1925
}
```

**Response** `204 No Content`

### Delete a Book

```http
DELETE /api/Books/1
```

**Response** `204 No Content`

---

## Seed Data

The database is pre-populated with the following books when migrations are applied:

| ID | Title | Author | Year Published |
|---|---|---|---|
| 1 | The Great Gatsby | F. Scott Fitzgerald | 1925 |
| 2 | To Kill a Mockingbird | Harper Lee | 1960 |
| 3 | 1984 | George Orwell | 1949 |

Seed data is defined in [`Data/FirstApiContext.cs`](Data/FirstApiContext.cs) using EF Core's `HasData()` method.

---

## Configuration

| Setting | Location | Description |
|---|---|---|
| Connection String | `appsettings.json` | PostgreSQL connection details |
| Logging | `appsettings.json` | Log level configuration |
| Launch Profiles | `Properties/launchSettings.json` | URLs, ports, and environment settings |

---

## Potential Improvements

- [ ] Add input validation with **Data Annotations** or **FluentValidation**
- [ ] Implement **pagination, filtering, and sorting** on the `GET /api/Books` endpoint
- [ ] Add **authentication & authorization** (e.g., JWT Bearer tokens)
- [ ] Introduce a **Service/Repository layer** to separate business logic from the controller
- [ ] Add **DTOs** (Data Transfer Objects) to decouple API contracts from entity models
- [ ] Write **unit and integration tests** with xUnit
- [ ] Add **Docker support** with a `docker-compose.yml` for the API and PostgreSQL
- [ ] Implement **global error handling** middleware
- [ ] Add **API versioning**
- [ ] Set up **CI/CD** with GitHub Actions

---

## License

This project is open source and available for learning purposes.