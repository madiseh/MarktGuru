using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OffersService.Data;
using System.Data.Common;

namespace OffersService.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext registrations for AppDbContext — both the options
            // object and the configuration delegates that carry the MySQL provider.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(DbConnection) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>) &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext)))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys=ON;";
                command.ExecuteNonQuery();
            }

            services.AddSingleton<DbCommandCounterInterceptor>();
            services.AddSingleton<DbConnection>(connection);
            services.AddDbContext<AppDbContext>((sp, options) =>
                options
                    .UseSqlite(sp.GetRequiredService<DbConnection>())
                    .AddInterceptors(sp.GetRequiredService<DbCommandCounterInterceptor>()));

            // Replace Hangfire's MySQL storage so the test host starts without a DB.
            services.AddHangfire(config => config.UseInMemoryStorage());
        });
    }
}
