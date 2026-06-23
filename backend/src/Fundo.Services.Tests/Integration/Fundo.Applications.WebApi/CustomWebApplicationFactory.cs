using Fundo.Applications.WebApi;
using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Services.Tests.Integration
{
    /// <summary>
    /// Boots the real API pipeline but replaces the SQL Server DbContext with an
    /// isolated in-memory database seeded from the model's HasData definition.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<LoanDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb"));

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
