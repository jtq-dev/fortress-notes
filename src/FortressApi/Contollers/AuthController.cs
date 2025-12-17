using FortressApi.Contracts;
using FortressApi.Data;
using FortressApi.Models;
using FortressApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortressApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(
        [FromServices] AppDbContext db,
        [FromServices] PasswordHasher hasher,
        [FromServices] TokenService tokens,
        [FromServices] AuditService audit,
        [FromBody] RegisterReq req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (email.Length < 5 || !email.Contains('@')) return BadRequest("Invalid email.");
        if (req.Password.Length < 10) return BadRequest("Password must be >= 10 chars.");

        var exists = await db.Users.AnyAsync(u => u.Email == email);
        if (exists) return Conflict("Email already registered.");

        var (hash, salt) = hasher.Hash(req.Password);
        var user = new User { Email = email, PasswordHash = hash, PasswordSalt = salt };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await audit.WriteAsync(HttpContext, user.Id, "REGISTER", "/api/auth/register");

        var jwt = tokens.CreateToken(user.Id, user.Email);
        return Ok(new AuthRes(jwt));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromServices] AppDbContext db,
        [FromServices] PasswordHasher hasher,
        [FromServices] TokenService tokens,
        [FromServices] AuditService audit,
        [FromBody] LoginReq req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized("Invalid credentials.");

        if (!hasher.Verify(req.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid credentials.");

        await audit.WriteAsync(HttpContext, user.Id, "LOGIN", "/api/auth/login");

        var jwt = tokens.CreateToken(user.Id, user.Email);
        return Ok(new AuthRes(jwt));
    }
}
