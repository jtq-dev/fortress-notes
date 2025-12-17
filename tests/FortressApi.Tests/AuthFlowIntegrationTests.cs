using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FortressApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public sealed class AuthFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task Register_Login_CreateNote_Works()
    {
        var client = _factory.CreateClient();

        var email = $"test{Guid.NewGuid():N}@example.com";
        var password = "VeryStrongPassword!!";

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterReq(email, password));
        reg.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginReq(email, password));
        login.EnsureSuccessStatusCode();

        var loginRes = await login.Content.ReadFromJsonAsync<AuthRes>();
        Assert.NotNull(loginRes);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginRes!.Token);

        var note = await client.PostAsJsonAsync("/api/notes", new NoteCreateReq("Hello", "World"));
        note.EnsureSuccessStatusCode();

        var noteRes = await note.Content.ReadFromJsonAsync<NoteRes>();
        Assert.NotNull(noteRes);
        Assert.Equal("Hello", noteRes!.Title);
    }
}
