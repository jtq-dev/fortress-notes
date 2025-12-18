using System.Net.Http.Headers;
using System.Net.Http.Json;
using FortressApi.Contracts;
using Xunit;

public sealed class AuthFlowIntegrationTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;

    public AuthFlowIntegrationTests(TestAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_CreateNote_Works()
    {
        var client = _factory.CreateClient();

        var email = $"test{Guid.NewGuid():N}@example.com";
        var password = "VeryStrongPassword!!";

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterReq(email, password));
        if (!reg.IsSuccessStatusCode)
        {
            var body = await reg.Content.ReadAsStringAsync();
            throw new Exception($"Register failed: {(int)reg.StatusCode} {reg.StatusCode}\n{body}");
        }
        var regRes = await reg.Content.ReadFromJsonAsync<AuthRes>();
        Assert.NotNull(regRes);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginReq(email, password));
        if (!login.IsSuccessStatusCode)
        {
            var body = await login.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {(int)login.StatusCode} {login.StatusCode}\n{body}");
        }
        var loginRes = await login.Content.ReadFromJsonAsync<AuthRes>();
        Assert.NotNull(loginRes);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginRes!.Token);

        var note = await client.PostAsJsonAsync("/api/notes", new NoteCreateReq("Hello", "World"));
        if (!note.IsSuccessStatusCode)
        {
            var body = await note.Content.ReadAsStringAsync();
            throw new Exception($"Create note failed: {(int)note.StatusCode} {note.StatusCode}\n{body}");
        }
        var noteRes = await note.Content.ReadFromJsonAsync<NoteRes>();
        Assert.NotNull(noteRes);
        Assert.Equal("Hello", noteRes!.Title);
    }
}
