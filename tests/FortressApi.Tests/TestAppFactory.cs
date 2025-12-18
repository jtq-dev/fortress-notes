using FortressApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private string? _dbPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        _dbPath = Path.Combine(Path.GetTempPath(), $"fortress_test_{Guid.NewGuid():N}.db");

        // ✅ Highest precedence + consistent in auth middleware
        Environment.SetEnvironmentVariable("Jwt__Issuer", "FortressNotes");
        Environment.SetEnvironmentVariable("Jwt__Audience", "FortressNotesUsers");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "TEST_SIGNING_KEY_32+_CHARS________________");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", $"Data Source={_dbPath}");

        builder.ConfigureServices(services =>
        {
            // Force EF to use test DB
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite($"Data Source={_dbPath}"));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
