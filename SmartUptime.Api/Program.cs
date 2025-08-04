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
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=smartuptime;Username=postgres;Password=postgres";
builder.Services.AddDbContext<SmartUptimeDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
        app.UseCors("AllowReactApp");

        app.MapControllers();

        // Initialize default emergency scripts
        using (var scope = app.Services.CreateScope())
        {
            var scriptRunner = scope.ServiceProvider.GetRequiredService<SmartUptime.Api.Services.ScriptRunnerService>();
            await scriptRunner.InitializeDefaultScriptsAsync();
        }

app.Run();
