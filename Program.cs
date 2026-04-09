using System.Text;
using FirstApi.Data;
using Microsoft.EntityFrameworkCore;
using FirstApi.Services;
using Microsoft.IdentityModel.Tokens;
using FirstApi.Repositories;
using FirstApi.Services.Interfaces;
using FirstApi.Repositories.Interfaces;
using FirstApi.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});
builder.Services.AddAuthorization();

builder.Services.AddDbContext<FirstApiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRateLimiter(options =>
{
    // If they get blocked, send back a 429 Too Many Requests
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Define a strict policy named "AuthLimit"
    options.AddFixedWindowLimiter("AuthLimit", config =>
    {
        // Only allow 5 requests per IP address...
        config.PermitLimit = 5;
        // ...every 1 minute.
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        // Don't queue extra requests, just block them instantly.
        config.QueueLimit = 0;
    });
});
var app = builder.Build();

// Automatically apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FirstApiContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
}

app.UseRateLimiter();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();   // ← BEFORE authorization
app.UseAuthorization();    // ← AFTER authentication


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Make Program accessible for integration tests (WebApplicationFactory<Program>)
public partial class Program { }
