using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SunumApp.EntityFrameworkCore;
using SunumApp.EntityFrameworkCore.Seed;
using SunumApp.Entities;

namespace SunumApp.Web.Host
{
    /// <summary>
    /// Background service that runs migration + seed once at startup
    /// without blocking the HTTP pipeline.
    /// </summary>
    public class MigrationHostedService : IHostedService
    {
        private readonly IConfiguration _config;

        public MigrationHostedService(IConfiguration config)
        {
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var connStr = _config.GetConnectionString("Default") ?? "";
            if (string.IsNullOrEmpty(connStr)) return;

            // Run in background to avoid blocking host startup (prevents EF tooling timeout)
            _ = Task.Run(async () =>
            {
                // Small delay to ensure host is fully started before DB operations
                await Task.Delay(1000, cancellationToken);
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseNpgsql(connStr);

                    using (var db = new SunumAppDbContext(optionsBuilder.Options))
                    {
                        db.Database.EnsureCreated();
                        Console.WriteLine("[Migration] Database is up to date.");

                        // Seed sample data — wrapped in its own try so a failure here doesn't block RBAC seed below.
                        try
                        {
                    if (!db.Aircrafts.Any())
                    {
                        db.Aircrafts.AddRange(
                    new Aircraft { Id = 1, Registration = "Sample Item 1", AircraftType = "Sample Item 1", Model = "Sample Item 1", Status = (Status)0 },
                    new Aircraft { Id = 2, Registration = "Sample Item 2", AircraftType = "Sample Item 2", Model = "Sample Item 2", Status = (Status)1 }
                        );
                    }
                    if (!db.Personnels.Any())
                    {
                        db.Personnels.AddRange(
                    new Personnel { Id = 3, FirstName = "Alice Johnson", LastName = "Alice Johnson", EmployeeNumber = "ABC-001", Role = (Role)0, LicenseNumber = "ABC-001" },
                    new Personnel { Id = 4, FirstName = "Bob Smith", LastName = "Bob Smith", EmployeeNumber = "XYZ-002", Role = (Role)1, LicenseNumber = "XYZ-002" }
                        );
                    }
                    if (!db.SnagReports.Any())
                    {
                        db.SnagReports.AddRange(
                    new SnagReport { Id = 5, ReportNumber = "ABC-001", AtaChapter = "Sample Item 1", Title = "Introduction to Physics", Description = "Lorem ipsum dolor sit amet", Severity = (Severity)0, DetectedAt = new DateTime(2024, 3, 15), ActionDescription = "Lorem ipsum dolor sit amet", RevisionNote = "Lorem ipsum dolor sit amet", Status = (Status)0, CertifyingStaffId = 1000L, AircraftId = 1, PersonnelId = 3 },
                    new SnagReport { Id = 6, ReportNumber = "XYZ-002", AtaChapter = "Sample Item 2", Title = "Advanced Mathematics", Description = "Consectetur adipiscing elit", Severity = (Severity)1, DetectedAt = new DateTime(2024, 6, 20), ActionDescription = "Consectetur adipiscing elit", RevisionNote = "Consectetur adipiscing elit", Status = (Status)1, CertifyingStaffId = 2000L, AircraftId = 2, PersonnelId = 4 }
                        );
                    }
                            db.SaveChanges();
                            Console.WriteLine("[Seed] Sample data created.");
                        }
                        catch (Exception sampleEx)
                        {
                            Console.WriteLine($"[Seed] Sample data skipped: {sampleEx.GetType().Name}: {sampleEx.Message}");
                            // Carry on — RBAC seed must still run so admin/123qwe is usable.
                        }
                    }
                    // RBAC seed (Admin/User roles + permissions + admin user) runs through ABP DI
                    // so PermissionRegistry can be injected. SeedHelper is idempotent.
                    SeedHelper.SeedHostDb(Abp.Dependency.IocManager.Instance);
                    Console.WriteLine("[Seed] RBAC seed complete (Admin role + admin user).");
                }
                catch (Exception ex)
                {
                    // Full diagnostic — surface the real cause so silent seed failures are debuggable.
                    Console.WriteLine($"[Migration] FAILED: {ex.GetType().Name}: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"[Migration] InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    Console.WriteLine("[Migration] StackTrace:");
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("[Migration] App continues without migration — admin user will not exist.");
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class Program
    {
        // Runtime entry: WebHost is required because ABP Startup returns IServiceProvider.
        public static void Main(string[] args)
        {
            // Npgsql 7+ requires UTC DateTimes — enable legacy behavior for ABP compatibility
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        // Design-time entry for EF Core tools (dotnet ef migrations).
        // Without this, EF tools wait 5 minutes for IHost build (resolver default timeout)
        // and then SIGTERM any running dotnet process — killing live dev servers.
        // We expose a minimal IHost that EF tools resolve in milliseconds; the actual
        // DbContext is built by IDesignTimeDbContextFactory in the EntityFrameworkCore project.
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);
    }
}
