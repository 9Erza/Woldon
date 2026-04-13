using Woldon.Models;
using Woldon.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICES REGISTRATION
// ==========================================

// Database: SQLite implementation with AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=woldon.db"));

// Security: Cookie-based Authentication setup
builder.Services.AddAuthentication("WoldonAuth")
    .AddCookie("WoldonAuth", options =>
    {
        options.Cookie.Name = "Woldon.Session";
        options.LoginPath = "/login.html"; // Unauthorized requests redirect here
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// MVC: Required for serving protected Razor views (Dashboard)
builder.Services.AddControllersWithViews();

// API Documentation: Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================
// 2. APPLICATION BUILD
// ==========================================
var app = builder.Build();

// ==========================================
// 3. STARTUP LOGIC (Database & Data Seeding)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    // Initial Database Setup: Creates schema if woldon.db doesn't exist
    db.Database.EnsureCreated();

    // Default Seed Data: Initialize admin account for first-time login
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = "admin123", // Consider Hashing for production
            WakePin = "1234"
        });
        db.SaveChanges();
    }
}

// ==========================================
// 4. MIDDLEWARE PIPELINE (Request Processing)
// ==========================================

// Static Files: Serves CSS, JS, and the public login page from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Security: Authentication must always come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Development Tools: Swagger UI for API testing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routing: Map all Controller endpoints (API & Views)
app.MapControllers();

// Execution: Start the Web Server
app.Run();