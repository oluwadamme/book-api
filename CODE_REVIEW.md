# Code Review & Improvement Notes

A review of the FirstApi codebase with specific, actionable improvements.

---

## What's Already Good

- Clean layered architecture: Controller → Service → Repository
- All database operations are async
- JWT + refresh token rotation
- OTP-based email verification and password reset
- Rate limiting on auth endpoints
- Global error handling middleware (`ExceptionMiddleware`)
- `BaseResponse<T>` wrapper for consistent API responses
- Docker support with auto-migrations
- Unit tests (Moq) + integration tests (WebApplicationFactory)
- Ownership-based access control on books

---

## Improvements

### 1. Expose a DTO for Books, Not the Entity Directly

**Problem:** `BooksController` accepts and returns the raw `Book` entity. This exposes `UserId` and `Id` in the request body — a client could try to send those fields. It also leaks internal DB structure.

**Fix:** Create a `CreateBookRequest` DTO with only the fields the client should supply:

```csharp
// DTOs/CreateBookRequest.cs
public class CreateBookRequest
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int YearPublished { get; set; }
}
```

Then update the controller action:
```csharp
[HttpPost]
public async Task<ActionResult<BaseResponse<Book>>> CreateBook(CreateBookRequest request)
{
    var userId = GetUserId();
    var book = new Book
    {
        Title = request.Title,
        Author = request.Author,
        YearPublished = request.YearPublished
    };
    var created = await bookService.AddBookAsync(book, userId);
    return CreatedAtAction(nameof(GetBook), new { id = created.Id },
        BaseResponse<Book>.SuccessResponse("Book created successfully", created));
}
```

Apply the same pattern to the `UpdateBook` endpoint with an `UpdateBookRequest` DTO.

---

### 2. Add Data Annotations for Input Validation

**Problem:** `RegisterRequest`, `LoginRequest`, and other DTOs have no `[Required]` attributes. If a client sends `null` for `Email` or `Password`, it reaches your service before being caught.

**Fix:** Use `System.ComponentModel.DataAnnotations` on all request DTOs. Since you already have `[ApiController]`, model validation runs automatically before your service is called.

```csharp
// DTOs/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
}
```

```csharp
// DTOs/LoginRequest.cs
public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }
}
```

Once `[Required]` and `[EmailAddress]` are in place, you can remove (or simplify) the manual `ValidateEmail` method in `AuthService` — the framework handles the null/format checks, and your service only needs to enforce business rules (like the special character password check).

---

### 3. Use `required` Modifier on Model Properties

**Problem:** `User.cs` and `Book.cs` have non-nullable `string` properties (`FirstName`, `Email`, `PasswordHash`, etc.) declared without the `required` modifier. This generates nullable warnings and allows accidentally constructing an invalid object.

**Fix:** Use the C# 11 `required` modifier:

```csharp
// Models/User.cs
public class User
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // nullable fields stay as-is
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
```

Do the same for `Title` and `Author` in `Book.cs`.

---

### 4. Use Strongly-Typed Configuration Options

**Problem:** Config values are read repeatedly with verbose, brittle code like `int.Parse(_config.GetSection("Jwt")["ExpirationInMinutes"]!)`. This pattern appears at least 4 times in `AuthService` alone.

**Fix:** Create options classes and bind them in `Program.cs`:

```csharp
// Options/JwtOptions.cs
public class JwtOptions
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; }
}

// Options/EmailVerificationOptions.cs
public class EmailVerificationOptions
{
    public int ExpirationInMinutes { get; set; }
}
```

Register them in `Program.cs`:
```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection("EmailVerification"));
```

Inject them into `AuthService`:
```csharp
public class AuthService(
    IAuthRepository authRepository,
    IOptions<JwtOptions> jwtOptions,
    IOptions<EmailVerificationOptions> emailOptions,
    IEmailService emailService) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly EmailVerificationOptions _emailVerification = emailOptions.Value;
    // ...
}
```

Now you can write `_jwt.ExpirationInMinutes` instead of parsing strings every time.

---

### 5. Fix the Seed Data (No UserId)

**Problem:** The seed books in `FirstApiContext` have `UserId = 0` (default int). Since all books endpoints filter by the logged-in user's ID, these seeded books are invisible to every user. If you later add a proper foreign key constraint (`Book.UserId` → `User.Id`), these records will violate it.

**Fix (Option A — remove seed data):** Remove the `HasData` call entirely. The seed books aren't useful when access is ownership-based.

**Fix (Option B — seed a user too):** Seed a system/admin user with a known ID and link the books to that user. This is more work but useful if you want demo data.

---

### 6. Uncomment the Email Verification Check

**Problem:** In `AuthService.LoginUserAsync` (lines 267–270), the email verification guard is commented out:

```csharp
// if (!user.IsEmailVerified)
// {
//     throw new ArgumentException("Email not verified");
// }
```

This means users can log in without ever verifying their email, which defeats the entire verification flow.

**Fix:** Uncomment it. If you need to bypass it during local development, use an environment check:
```csharp
if (!user.IsEmailVerified && !_environment.IsDevelopment())
{
    throw new ArgumentException("Email not verified. Please check your inbox.");
}
```
(Inject `IHostEnvironment` into `AuthService` for this.)

---

### 7. Fix HTTP Status Codes

| Endpoint | Current | Should Be | Reason |
|---|---|---|---|
| `POST /api/Auth/register` | `200 OK` | `201 Created` | A new user resource was created |
| `DELETE /api/Books/{id}` | `200 OK` | `204 No Content` | Successful deletion has no response body |

The `POST /api/Books` endpoint already returns `201 Created` correctly — apply the same pattern to register.

---

### 8. Log Fire-and-Forget Email Failures

**Problem:** `_ = _emailService.SendEmailAsync(...)` discards the task entirely. If sending fails, it's silently swallowed — no log, no trace.

**Fix:** Attach a continuation to log failures without blocking the request:
```csharp
_ = _emailService.SendEmailAsync(user.Email, user.FirstName, subject, body)
    .ContinueWith(
        t => _logger.LogError(t.Exception, "Failed to send email to {Email}", user.Email),
        TaskContinuationOptions.OnlyOnFaulted);
```

You'll need to inject `ILogger<AuthService>` into `AuthService`. This is also a good general practice — services that do meaningful work should log.

---

### 9. Define the Foreign Key Relationship Explicitly

**Problem:** `Book.UserId` references a `User`, but there's no explicit FK relationship in `OnModelCreating`. EF Core infers it by convention, but it's better to be explicit — it documents intent and lets you control cascade behaviour.

**Fix:** Add to `FirstApiContext.OnModelCreating`:
```csharp
modelBuilder.Entity<Book>()
    .HasOne<User>()
    .WithMany()
    .HasForeignKey(b => b.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

This also means deleting a user will automatically delete all their books.

---

### 10. Improve Email Validation

**Problem:** `ValidateEmail` in `AuthService` only checks for the presence of `@` and `.`. Strings like `a@b` or `@.com` would pass.

**Fix (if keeping manual validation):** Use `System.Net.Mail.MailAddress` which does proper RFC validation:
```csharp
private bool IsValidEmail(string email)
{
    try
    {
        _ = new System.Net.Mail.MailAddress(email);
        return true;
    }
    catch (FormatException)
    {
        return false;
    }
}
```

**Better fix:** Use `[EmailAddress]` data annotation on the DTOs (see item 2) and remove the manual check entirely. The framework handles it.

---

### 11. Document the Logout Endpoint in the README

**Problem:** `AuthController` has a `POST /api/Auth/logout` endpoint that revokes the refresh token, but it's not documented anywhere in the README's API endpoints table.

**Fix:** Add it to the Authentication API table in `README.md`:

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/Auth/logout` | Revoke refresh token (invalidates session) | ❌ No |

---

## Remaining TODOs (from README)

- **API Versioning** — Use the `Asp.Versioning.Http` NuGet package. Route-based versioning (`/api/v1/books`) is the most common approach.
- **CI/CD with GitHub Actions** — Start with a simple workflow: on every push to `main`, run `dotnet build` and `dotnet test`. Add a `.github/workflows/ci.yml` file.

---

## Summary

The architecture is solid. The main themes for improvement are:

1. **Never expose entity models directly in API contracts** — use request/response DTOs
2. **Let data annotations do input validation** — `[Required]`, `[EmailAddress]` instead of manual string checks
3. **Strongly-typed config** — bind config sections to options classes instead of parsing raw strings
4. **Activate the email verification check** — it's there but commented out
