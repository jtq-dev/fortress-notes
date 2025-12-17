using FortressApi.Contracts;
using FortressApi.Data;
using FortressApi.Models;
using FortressApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FortressApi.Controllers;

[ApiController]
[Authorize]
[Route("api/notes")]
public sealed class NotesController : ControllerBase
{
    private static Guid UserId(ClaimsPrincipal u)
        => Guid.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<NoteRes>>> List(
        [FromServices] AppDbContext db,
        int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);
        var uid = UserId(User);

        var notes = await db.Notes
            .Where(n => n.UserId == uid)
            .OrderByDescending(n => n.UpdatedAtUtc)
            .Take(limit)
            .Select(n => new NoteRes(n.Id, n.Title, n.Body, n.UpdatedAtUtc))
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPost]
    public async Task<ActionResult<NoteRes>> Create(
        [FromServices] AppDbContext db,
        [FromServices] AuditService audit,
        [FromBody] NoteCreateReq req)
    {
        var uid = UserId(User);
        if (req.Title.Length is < 1 or > 120) return BadRequest("Title must be 1..120 chars.");
        if (req.Body.Length is > 4000) return BadRequest("Body too long.");

        var note = new Note { UserId = uid, Title = req.Title.Trim(), Body = req.Body.Trim() };
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        await audit.WriteAsync(HttpContext, uid, "NOTE_CREATED", $"/api/notes/{note.Id}");

        return Ok(new NoteRes(note.Id, note.Title, note.Body, note.UpdatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoteRes>> Update(
        [FromServices] AppDbContext db,
        [FromServices] AuditService audit,
        [FromRoute] Guid id,
        [FromBody] NoteUpdateReq req)
    {
        var uid = UserId(User);

        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == id && n.UserId == uid);
        if (note is null) return NotFound();

        note.Title = req.Title.Trim();
        note.Body = req.Body.Trim();
        note.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        await audit.WriteAsync(HttpContext, uid, "NOTE_UPDATED", $"/api/notes/{note.Id}");

        return Ok(new NoteRes(note.Id, note.Title, note.Body, note.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromServices] AppDbContext db,
        [FromServices] AuditService audit,
        [FromRoute] Guid id)
    {
        var uid = UserId(User);
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == id && n.UserId == uid);
        if (note is null) return NotFound();

        db.Notes.Remove(note);
        await db.SaveChangesAsync();

        await audit.WriteAsync(HttpContext, uid, "NOTE_DELETED", $"/api/notes/{id}");
        return NoContent();
    }
}
