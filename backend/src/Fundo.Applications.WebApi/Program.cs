using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fundo.Applications.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting Fundo WebApi");

                var host = CreateWebHostBuilder(args).Build();
                ApplyMigrations(host);
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled WebApi exception during startup");
            }
            finally
            {
                Log.Information("Application shutting down");
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog(Log.Logger, dispose: true);
                })
                .UseStartup<Startup>();
        }

        private static void ApplyMigrations(IWebHost host)
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();

            // SQL Server may still be warming up when running under Docker Compose;
            // retry a handful of times before giving up.
            const int maxAttempts = 10;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    dbContext.Database.Migrate();
                    Log.Information("Database migrations applied successfully");
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Log.Warning(ex, "Database not ready (attempt {Attempt}/{Max}); retrying in 5s",
                        attempt, maxAttempts);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
