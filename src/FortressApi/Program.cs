using FortressApi.Data;
using FortressApi.Middleware;
using FortressApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Fortress Notes API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    o.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    opts.UseSqlite(cs);
});

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuditService>();

// Rate limiting (basic but real)
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 30;
        opt.QueueLimit = 0;
    });
});

// JWT Auth
var key = builder.Configuration["Jwt:SigningKey"] ?? "";
if (key.Length < 32) Console.WriteLine("WARNING: Jwt:SigningKey should be >= 32 chars (set via env in prod).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Create DB, etc...

// ✅ DON’T force HTTPS redirect on Render (optional)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ✅ Enable Swagger in ALL environments (so recruiters can test)
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fortress Notes API v1");
    c.RoutePrefix = "swagger"; // ✅ /swagger
});


app.UseRateLimiter();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
app.MapGet("/", () => Results.Content("""
<!doctype html>
<html>
<head>
  <meta charset="utf-8"/>
  <meta name="viewport" content="width=device-width,initial-scale=1"/>
  <title>Fortress Notes API</title>
  <style>
    body{font-family:system-ui;margin:0;background:#0b0b10;color:#fff}
    .wrap{max-width:920px;margin:0 auto;padding:48px}
    .card{background:#141422;border:1px solid #2a2a3b;border-radius:16px;padding:20px}
    a.btn{display:inline-block;margin:10px 10px 0 0;padding:12px 16px;border-radius:12px;text-decoration:none;font-weight:700}
    .p{color:#c9c9d6;line-height:1.6}
    .btn1{background:#7c3aed;color:#fff}
    .btn2{background:#fff;color:#111}
    code{background:#1f1f2e;padding:2px 6px;border-radius:8px}
  </style>
</head>
<body>
  <div class="wrap">
    <h1>Fortress Notes API</h1>
    <p class="p">Secure CRUD API with JWT, secure headers, rate limiting, tests, Docker, CI.</p>

    <div class="card">
      <h3>Try it</h3>
      <p class="p">Use the dev console to register/login and test endpoints.</p>
      <a class="btn btn1" href="/swagger">Open Swagger (Dev Console)</a>
      <a class="btn btn2" href="/health/live">Health Check</a>
      <p class="p" style="margin-top:14px">
        Tip: In Swagger click <b>Authorize</b> and paste <code>Bearer &lt;token&gt;</code>.
      </p>
    </div>
  </div>
</body>
</html>
""", "text/html"));

app.MapGet("/health/live", () => Results.Ok(new { ok = true }));
app.MapGet("/health/ready", () => Results.Ok(new { ok = true }));

app.Run();


// For integration tests
public partial class Program { }
