using System.Text;
using EmployeeDepartment.Data;
using EmployeeDepartment.Interfaces;
using EmployeeDepartment.Jobs;
using EmployeeDepartment.Services;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EmployeeDepartment API",
        Version = "v1",
        Description = "ASP.NET Core Web API for Employee Department assignment."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Auth Failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token Validated: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ProjectReminderJob>();

builder.Services.AddHangfire(config =>
    config.UseStorage(
        new MySqlStorage(
            connectionString,
            new MySqlStorageOptions
            {
                TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "Hangfire"
            })));

builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    // Seed default users
    SeedDefaultUsers(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Debug middleware to log Authorization header
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"[DEBUG] Request {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"[DEBUG] Authorization Header: {authHeader}");
        Console.WriteLine($"[DEBUG] Header Length: {authHeader.Length}");
        Console.WriteLine($"[DEBUG] Contains 'Bearer ': {authHeader.StartsWith("Bearer ")}");
        if (authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring(7);
            Console.WriteLine($"[DEBUG] Token Length: {token.Length}");
            Console.WriteLine($"[DEBUG] Token contains dots: {token.Contains('.')}");
            Console.WriteLine($"[DEBUG] Dot count: {token.Split('.').Length - 1}");
        }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<ProjectReminderJob>(
    "project-reminder-job",
    job => job.CheckProjectsAsync(),
    Cron.Daily);

app.MapControllers();

// Seed default users method
static void SeedDefaultUsers(AppDbContext db)
{
    try
    {
        // Ensure database is created first
        db.Database.EnsureCreated();
        
        if (!db.Users.Any(u => u.UserName == "admin"))
        {
            EmployeeDepartment.Helpers.PasswordHelper.CreatePasswordHash("admin123", out var adminHash, out var adminSalt);
            db.Users.Add(new EmployeeDepartment.Models.AppUser
            {
                FullName = "System Administrator",
                UserName = "admin",
                Email = "admin@employeeapi.com",
                Role = "Admin",
                PasswordHash = adminHash,
                PasswordSalt = adminSalt
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.UserName == "user"))
        {
            EmployeeDepartment.Helpers.PasswordHelper.CreatePasswordHash("user123", out var userHash, out var userSalt);
            db.Users.Add(new EmployeeDepartment.Models.AppUser
            {
                FullName = "Default User",
                UserName = "user",
                Email = "user@employeeapi.com",
                Role = "User",
                PasswordHash = userHash,
                PasswordSalt = userSalt
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not seed default users: {ex.Message}");
    }
}

app.Run();