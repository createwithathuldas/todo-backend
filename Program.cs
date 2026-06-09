using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using Todo_backend.Data;
using Todo_backend.Services;
using Todo_backend.Controllers;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionPath = builder.Configuration["DataProtection:KeyPath"] ?? Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(dataProtectionPath);

builder.Services.AddDataProtection()
    .SetApplicationName("TodoBackend")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=TodoDB;User=root;Password=;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString)
);

// Add JWT Service
builder.Services.AddScoped<JwtService>();

// Add Todo Service
builder.Services.AddScoped<ITodoService, TodoService>();

// Add controllers
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins("http://localhost:4200", "http://localhost:4201")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-change-this-in-production-to-at-least-32-characters";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Todo.Backend";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Todo.Frontend";

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Use CORS policy
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();