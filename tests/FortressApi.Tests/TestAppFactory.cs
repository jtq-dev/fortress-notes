using FortressApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private string? _dbPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"fortress_test_{Guid.NewGuid():N}.db");

            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "FortressNotes",
                ["Jwt:Audience"] = "FortressNotesUsers",
                ["Jwt:SigningKey"] = "TEST_SIGNING_KEY_32+_CHARS________________",
                ["ConnectionStrings:Default"] = $"Data Source={_dbPath}"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Make sure EF uses the test DB (same connection string we injected)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite($"Data Source={_dbPath}"));

            // Build provider and ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
