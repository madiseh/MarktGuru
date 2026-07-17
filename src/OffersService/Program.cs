using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.IOC;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// EF Core with Pomelo MySQL
var serverVersion = new MariaDbServerVersion(new Version(10, 11, 0));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        TablesPrefix = "Hangfire_"
    })));
builder.Services.AddHangfireServer();

builder.Services.AddApplicationServices();
builder.Services.AddApplicationJobs();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplicationExceptionHandling();

var app = builder.Build();

// Apply EF Core migrations (creates DB and tables if they don't exist).
// In-memory provider (used in tests) doesn't support Migrate — use EnsureCreated instead.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsEnvironment("Test"))
        await db.Database.EnsureCreatedAsync();
    else if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // available at /openapi/v1.json
}

app.UseApplicationExceptionHandling();
app.UseHangfireDashboard("/hangfire");
app.MapControllers();
app.UseApplicationRecurringJobs();

app.Run();

public partial class Program { }
