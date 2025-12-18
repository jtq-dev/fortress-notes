using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"fortress_test_{Guid.NewGuid():N}.db");

            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "FortressNotes",
                ["Jwt:Audience"] = "FortressNotesUsers",
                // MUST be 32+ chars
                ["Jwt:SigningKey"] = "TEST_SIGNING_KEY_32+_CHARS________________",
                ["ConnectionStrings:Default"] = $"Data Source={dbPath}"
            });
        });
    }
}
