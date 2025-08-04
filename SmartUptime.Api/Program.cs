using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartUptime.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddHostedService<SmartUptime.Api.Services.SitePingBackgroundService>();
        builder.Services.AddScoped<SmartUptime.Api.Services.ScriptRunnerService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://smart-uptime-sheshanth-system.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=smartuptime.db";
var isProduction = builder.Environment.IsProduction();

builder.Services.AddDbContext<SmartUptimeDbContext>(options =>
{
    if (isProduction && connectionString.Contains("Host="))
    {
        // Use PostgreSQL in production
        options.UseNpgsql(connectionString);
    }
    else
    {
        // Use SQLite for local development
        options.UseSqlite(connectionString);
    }
});

var app = builder.Build();

// 1. Run migrations FIRST
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartUptimeDbContext>();
    await db.Database.MigrateAsync();
}

// 2. Now initialize default scripts
await using (var scope = app.Services.CreateAsyncScope())
{
    var scriptRunner = scope.ServiceProvider.GetRequiredService<SmartUptime.Api.Services.ScriptRunnerService>();
    await scriptRunner.InitializeDefaultScriptsAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
app.UseCors("AllowReactApp");

app.MapControllers();

await app.RunAsync();
